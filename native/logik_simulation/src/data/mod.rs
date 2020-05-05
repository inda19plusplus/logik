use std::collections::{HashMap, HashSet, BinaryHeap};
use std::collections::hash_map::Entry;

use crate::data::component::Component;
use crate::data::subnet::Subnet;
use std::cmp::Reverse;

pub(crate) mod subnet;
pub(crate) mod component;

/// Struct to represent the data that the backend should keep track of
#[derive(Debug)]
pub struct Data {
    components: HashMap<i32, Box<dyn Component>>,
    components_free: BinaryHeap<Reverse<i32>>,
    subnets: HashMap<i32, Subnet>,
    // <id, subnet>
    edges: HashMap<i32, HashSet<Connection>>, // key is from node, value is to node
    // components live on odd indices
    // subnets live on even indices
}

impl Data {
    pub fn new() -> Self {
        Self {
            components: HashMap::new(),
            components_free: BinaryHeap::new(),
            subnets: HashMap::new(),
            edges: HashMap::new(),
        }
    }
    
    fn alloc_component(&mut self, component: Box<dyn Component>) -> i32 {
        let idx = if self.components_free.len() == 0 {
            self.components.len() as i32 + 1
        } else {
            self.components_free.pop().unwrap().0
        };
    
        assert!(self.components.insert(idx, component).is_none());
        idx
    }
    
    pub(crate) fn add_component(&mut self, component: Box<dyn Component>, inputs: Vec<Option<i32>>, outputs: Vec<Option<i32>>) -> Result<i32, ()> {
        if component.inputs() != inputs.len() || component.outputs() != outputs.len() {
            return Err(());
        }
        
        let idx = self.alloc_component(component);
        
        for (port, input) in inputs.into_iter()
            .enumerate()
            .filter_map(|(i, e)| e.map(|e| (i, e * 2))) {
            self.edges.entry(input).or_default().insert(Connection::Component(2 * idx + 1, port));
        }
        
        let filtered = outputs
            .into_iter()
            .filter_map(|e| e.map(|e| Connection::Subnet(e * 2)))
            .collect::<Vec<_>>();
        
        if filtered.len() > 0 {
            self.edges
                .entry(2 * idx + 1)
                .or_default()
                .extend(filtered);
        }
        
        Ok(idx)
    }
    
    pub(crate) fn remove_component(&mut self, id: i32) -> bool {
        let component = match self.components.remove(&id) {
            Some(c) => c,
            None => return false,
        };
        
        self.components_free.push(Reverse(id));
        
        self.edges.remove(&(2 * id + 1));
    
        for i in self.subnets.keys().map(|e| *e * 2) {
            if let Entry::Occupied(mut inner) = self.edges.entry(i) {
                for input in 0..component.inputs() {
                    inner.get_mut().remove(&Connection::Component(2 * id + 1, input));
                }
            }
        }
        
        true
    }
    
    pub(crate) fn add_subnet(&mut self, id: i32) -> bool {
        self.subnets.insert(id, Subnet::new()).is_none()
    }
    
    pub(crate) fn remove_subnet(&mut self, id: i32) -> bool {
        if self.subnets.remove(&id).is_none() {
            return false;
        }
    
        let id = 2 * id;
        
        self.edges.remove(&id);
        
        for i in (0..self.components.len() as i32).map(|e| 2 * e + 1) {
            if let Entry::Occupied(mut inner) = self.edges.entry(i) {
                inner.get_mut().remove(&Connection::Subnet(id));
            }
        }
        
        true
    }
    
    pub(crate) fn link(&mut self, component: i32, port: usize, subnet: i32, direction: bool) -> bool {
        //true is component to subnet, false is subnet to component
        !if direction {
            self.edges.entry(component * 2 + 1).or_default().insert(Connection::Subnet(subnet * 2))
        } else {
            self.edges.entry(subnet * 2).or_default().insert(Connection::Component(component * 2 + 1, port))
        }
    }
    
    pub(crate) fn unlink(&mut self, component: i32, port: usize, subnet: i32) -> bool {
        let mut found = false;
        
        if let Entry::Occupied(mut inner) = self.edges.entry(component * 2 + 1) {
            if inner.get_mut().remove(&Connection::Subnet(subnet * 2)) {
                found = true;
            }
        }
    
        if let Entry::Occupied(mut inner) = self.edges.entry(subnet * 2) {
            if inner.get_mut().remove(&Connection::Component(component * 2 + 1, port)) {
                found = true;
            }
        }
        
        found
    }
}

#[derive(Debug, Eq, PartialEq, Clone, Hash)]
pub enum Connection {
    Subnet(i32),
    Component(i32, usize),
}

#[cfg(test)]
mod test {
    use crate::data::component::{AND, Output};
    
    use super::*;
    
    macro_rules! map (
        ( $($key:expr => $value:expr),+ ) => {
            {
                let mut m = ::std::collections::HashMap::new();
                $(
                    m.insert($key, $value);
                )+
                m
            }
        };
        () => {
            {
                ::std::collections::HashMap::new()
            }
        };
    );
    
    macro_rules! set {
        ( $($val:expr),+ ) => {
            {
                let mut s = ::std::collections::HashSet::new();
                $(
                    s.insert($val);
                )+
                s
            }
        };
        () => {
            {
                ::std::collections::HashSet::new();
            }
        };
    }
    
    #[test]
    fn test_adding_components() {
        let mut data = Data::new();
        
        data.add_subnet(0);
        
        assert!(data.add_component(Box::new(Output {}), vec![Some(0)], vec![]).is_ok());
        
        data.add_subnet(1);
        data.add_subnet(5);
        
        assert!(data.add_component(Box::new(AND {}), vec![Some(1), Some(5)], vec![Some(0)]).is_ok());
        
        assert!(data.add_component(Box::new(Output {}), vec![Some(0)], vec![]).is_ok());
        
        assert_eq!(data.edges, map!(
            2 => set!(Connection::Component(3, 0)),
            10 => set!(Connection::Component(3, 1)),
            3 => set!(Connection::Subnet(0)),
            0 => set!(Connection::Component(1, 0), Connection::Component(5, 0))
        ));
        
        assert!(data.add_component(Box::new(AND {}), vec![], vec![]).is_err());
    }
    
    #[test]
    fn test_removing_subnets() {
        let mut data = Data::new();
        
        data.add_subnet(0);
        data.add_subnet(1);
        
        assert_eq!(data.edges, map!());
        
        assert!(data.add_component(Box::new(Output {}), vec![Some(0)], vec![]).is_ok());
        
        assert_eq!(data.edges, map!(0 => set!(Connection::Component(1, 0))));
        
        assert!(data.remove_subnet(0));
        
        assert_eq!(data.edges, map!());
        
        assert!(data.remove_subnet(1));
    
        assert_eq!(data.edges, map!());
        
        assert!(!data.remove_subnet(0));
        assert!(!data.remove_subnet(3));
    }
}