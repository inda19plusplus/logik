use std::collections::{HashMap, HashSet};
use std::collections::hash_map::Entry;

use crate::data::component::Component;
use crate::data::subnet::Subnet;

mod subnet;
mod component;

/// Struct to represent the data that the backend should keep track of
pub struct Data {
    components: Vec<Box<dyn Component>>,
    subnets: HashMap<usize, Subnet>,
    // <id, subnet>
    edges: HashMap<usize, HashSet<usize>>, // key is from node, value is to node
    // components live on odd indices
    // subnets live on even indices
}

impl Data {
    pub fn new() -> Self {
        Self {
            components: Vec::new(),
            subnets: HashMap::new(),
            edges: HashMap::new(),
        }
    }
    
    pub(crate) fn component(&self, idx: usize) -> Option<&dyn Component> {
        Some(&**self.components.get(idx)?)
    }
    
    pub(crate) fn component_mut(&mut self, idx: usize) -> Option<&mut dyn Component> {
        Some(&mut **self.components.get_mut(idx)?)
    }
    
    pub(crate) fn subnet(&self, idx: usize) -> Option<&Subnet> {
        Some(self.subnets.get(&idx)?)
    }
    
    pub(crate) fn subnet_mut(&mut self, idx: usize) -> Option<&mut Subnet> {
        Some(self.subnets.get_mut(&idx)?)
    }
    
    pub(crate) fn add_component(&mut self, component: Box<dyn Component>, inputs: Vec<usize>, outputs: Vec<usize>) -> bool {
        if component.inputs() != inputs.len() || component.outputs() != outputs.len() {
            return false;
        }
        
        for input in inputs.into_iter().map(|e| e * 2) {
            self.edges.entry(input).or_default().insert(2 * self.components.len() + 1);
        }
        
        if outputs.len() > 0 {
            self.edges
                .entry(2 * self.components.len() + 1)
                .or_default()
                .extend(outputs
                    .into_iter()
                    .map(|e| e * 2)
                );
        }
        
        self.components.push(component);
        
        true
    }
    
    pub(crate) fn add_subnet(&mut self, id: usize) -> bool {
        match self.subnets.insert(id, Subnet::new()) {
            Some(_) => false,
            None => true,
        }
    }
    
    pub(crate) fn remove_subnet(&mut self, id: usize) -> bool {
        if self.edges.remove(&id).is_none() {
            return false;
        }
        
        let id = 2 * id;
        for i in (0..self.components.len()).map(|e| 2 * e + 1) {
            if let Entry::Occupied(mut inner) = self.edges.entry(i) {
                inner.get_mut().remove(&id);
            }
        }
        
        true
    }
}

#[cfg(test)]
mod test {
    use std::iter::FromIterator;
    
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
        
        assert!(data.add_component(Box::new(Output {}), vec![0], vec![]));
        
        data.add_subnet(1);
        data.add_subnet(5);
        
        assert!(data.add_component(Box::new(AND {}), vec![1, 5], vec![0]));
        
        assert!(data.add_component(Box::new(Output {}), vec![0], vec![]));
        
        assert_eq!(data.edges, map!(2 => set!(3), 10 => set!(3), 3 => set!(0), 0 => set!(1, 5)));
    }
    
    #[test]
    fn test_removing_subnets() {
        let mut data = Data::new();
        
        data.add_subnet(0);
        data.add_subnet(1);
        
        assert_eq!(data.edges, map!());
        
        assert!(data.add_component(Box::new(Output {}), vec![0], vec![]));
        
        assert_eq!(data.edges, map!(0 => set!(1)));
        
        assert!(data.remove_subnet(0));
        
        assert_eq!(data.edges, map!());
    }
}