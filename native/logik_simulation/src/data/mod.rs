use std::collections::{HashMap, HashSet, BinaryHeap, VecDeque};
use std::collections::hash_map::Entry;

use crate::data::component::{Component, PortType};
use crate::data::subnet::{Subnet, SubnetState};
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
    edges: HashMap<i32, HashSet<Edge>>, // key is node, value is all edges associated with the node
    // components live on odd indices
    // subnets live on even indices
    dirty_subnets: VecDeque<Vec<i32>>,
}

impl Data {
    pub fn new() -> Self {
        Self {
            components: HashMap::new(),
            components_free: BinaryHeap::new(),
            subnets: HashMap::new(),
            edges: HashMap::new(),
            dirty_subnets: VecDeque::new(),
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
    
    pub(crate) fn add_component(&mut self, component: Box<dyn Component>, ports: Vec<Option<i32>>) -> Result<i32, ()> {
        if ports.len() != component.ports() {
            return Err(());
        }
        
        let ports = ports.into_iter()
            .enumerate()
            .filter_map(|(i, e)| e.map(|e| (component.port_type(i).unwrap(), i, e)))
            .collect::<Vec<_>>();
        
        let idx = self.alloc_component(component);
    
        for port in ports {
            self.add_edge(port.2, idx, port.1, port.0.into());
        }
        
        Ok(idx)
    }
    
    pub(crate) fn remove_component(&mut self, id: i32) -> bool {
        if self.components.remove(&id).is_none() {
            return false
        };
        
        self.components_free.push(Reverse(id));
    
        let mut to_remove = Vec::new();
        
        for edge in self.edges.get(&id).unwrap() {
            to_remove.push(edge.clone());
        }
    
        for r in to_remove {
            self.remove_edge(&r);
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
    
        if let Some(edges) = self.edges.get(&(2 * id)) {
            let mut to_remove = Vec::new();
    
            for edge in edges {
                to_remove.push(edge.clone());
            }
    
            for r in to_remove {
                self.remove_edge(&r);
            }
        }
        
        true
    }
    
    pub(crate) fn link(&mut self, component: i32, port: usize, subnet: i32) -> bool {
        let direction = match self.port_direction_component(component, port) {
            Some(t) => t,
            None => return false,
        };
        
        self.add_edge(subnet, component, port, direction);
        
        true
    }
    
    pub(crate) fn unlink(&mut self, component: i32, port: usize, subnet: i32) -> bool {
        let direction = match self.port_direction_component(component, port) {
            Some(t) => t,
            None => return false,
        };
        
        self.remove_edge(&Edge::new(subnet, component, port, direction));
        
        true
    }
    
    fn add_edge(&mut self, subnet: i32, component: i32, port: usize, direction: EdgeDirection) {
        let edge = Edge::new(subnet, component, port, direction);
    
        let mut removing = Vec::new();
        if let Entry::Occupied(inner) = self.edges.entry(2 * subnet) {
            let same = inner.get().iter().find(|e| e.same_nodes(&edge));
            if let Some(same) = same { //This edge already exists
                removing.push(same.clone());
            }
        }
    
        for r in removing {
            self.remove_edge(&r);
        }
        
        // here, we're guaranteed to not have an edge between the subnet and the component's port
        // so we can just put in edges without worry of collision
        self.edges.entry(2 * component + 1).or_default().insert(edge.clone());
        self.edges.entry(2 * subnet).or_default().insert(edge);
    }
    
    fn remove_edge(&mut self, edge: &Edge) {
        self.edges.get_mut(&(edge.component)).unwrap().remove(edge);
        self.edges.get_mut(&(edge.subnet)).unwrap().remove(edge);
        if self.edges.get(&(edge.component)).unwrap().len() == 0 {
            self.edges.remove(&(edge.component));
        }
        if self.edges.get(&(edge.subnet)).unwrap().len() == 0 {
            self.edges.remove(&(edge.subnet));
        }
    }
    
    fn port_direction_component(&self, component: i32, port: usize) -> Option<EdgeDirection> {
        Some(self.components.get(&component)?.port_type(port)?.to_edge_direction())
    }
    
    pub(crate) fn advance_time(&mut self) {
        let to_simulate = match self.dirty_subnets.pop_front() {
            Some(t) => t,
            None => return,
        };
    
        let mut simulating = HashSet::new();
        for subnet in to_simulate {
            for edge in self.edges.get(&(2 * subnet)).unwrap() {
                if edge.direction != EdgeDirection::ToSubnet {
                    simulating.insert((edge.component - 1) / 2);
                }
            }
        }
    
        for s in simulating {
            self.simulate(s);
        }
    }
    
    fn simulate(&mut self, component: i32) {
        let comp = &**self.components.get(&component).unwrap();
        let edges = self.edges.get(&(2 * component + 1)).unwrap();
        let mut searching = HashSet::new();
        let mut dirty_ports = HashSet::new();
        for (port_idx, port_type) in comp.ports_type()
            .into_iter()
            .enumerate()
        {
            if port_type != PortType::Output {
                searching.insert(port_idx);
            } else if port_type != PortType::Input {
                dirty_ports.insert(port_idx);
            }
        }
    
        let mut states = HashMap::new();
        let mut dirtying = HashMap::new();
        
        for edge in edges {
            if searching.contains(&edge.port) {
                let val = self.subnets.get(&(edge.subnet / 2)).unwrap().val();
                states.insert(edge.port, val);
            } else if dirty_ports.contains(&edge.port) {
                dirtying.insert(edge.port, edge.subnet / 2);
            }
        }
        
        if states.len() != searching.len() {
            return;
        }
        
        let res = comp.evaluate(states).unwrap();
    
        for (port, state) in res {
            let subnet = dirtying.get(&port).unwrap();
            self.update_subnet(*subnet, state);
        }
    }
    
    fn update_subnet(&mut self, subnet: i32, state: SubnetState) {
        self.subnets.get_mut(&subnet).unwrap().update(state);
        if self.dirty_subnets.len() == 0 { //maybe change to account for propagation time
            self.dirty_subnets.push_back(Vec::new());
        }
        self.dirty_subnets.get_mut(0).unwrap().push(subnet);
    }
}

#[derive(Debug, Eq, PartialEq, Clone, Hash)]
pub enum Connection {
    Subnet(i32, usize),
    Component(i32, usize),
}

#[derive(Debug, Eq, PartialEq, Clone, Hash)]
struct Edge {
    subnet: i32,
    component: i32,
    port: usize,
    direction: EdgeDirection,
}

impl Edge {
    fn new(subnet: i32, component: i32, port: usize, direction: EdgeDirection) -> Self {
        Self {
            subnet: 2 * subnet,
            component: 2 * component + 1,
            port, direction
        }
    }
    
    fn same_nodes(&self, other: &Self) -> bool {
        self.subnet == other.subnet &&
            self.component == other.component &&
            self.port == other.port
    }
    
    fn add_direction(mut self, direction: EdgeDirection) -> Self {
        if self.direction == direction {
            self
        } else {
            self.direction = EdgeDirection::Bidirectional;
            self
        }
    }
}

#[derive(Debug, Eq, PartialEq, Clone, Copy, Hash)]
pub(crate) enum EdgeDirection {
    ToComponent,
    ToSubnet,
    Bidirectional,
}

#[cfg(test)]
mod test {
    use crate::data::component::{AND, Output, NOT};
    use crate::{map, set};
    use super::*;
    
    macro_rules! edge {
        ($subnet:expr, $component:expr, $port:expr, 0) => {
            Edge {
                subnet: $subnet,
                component: $component,
                port: $port,
                direction: EdgeDirection::ToComponent,
            }
        };
        ($subnet:expr, $component:expr, $port:expr, 1) => {
            Edge {
                subnet: $subnet,
                component: $component,
                port: $port,
                direction: EdgeDirection::Bidirectional,
            }
        };
        ($subnet:expr, $component:expr, $port:expr, 2) => {
            Edge {
                subnet: $subnet,
                component: $component,
                port: $port,
                direction: EdgeDirection::ToSubnet,
            }
        };
    }
    
    #[test]
    fn test_adding_components() {
        let mut data = Data::new();
        
        data.add_subnet(0);
        
        assert!(data.add_component(Box::new(Output {}), vec![Some(0)]).is_ok());
        
        data.add_subnet(1);
        data.add_subnet(5);
        
        assert!(data.add_component(Box::new(AND {}), vec![Some(1), Some(5), Some(0)]).is_ok());
        
        assert!(data.add_component(Box::new(Output {}), vec![Some(0)]).is_ok());
        
        assert_eq!(data.edges, map!(
            3 => set!(edge!(0, 3, 0, 0)),
            0 => set!(edge!(0, 3, 0, 0), edge!(0, 5, 2, 2), edge!(0, 7, 0, 0)),
            5 => set!(edge!(0, 5, 2, 2), edge!(2, 5, 0, 0), edge!(10, 5, 1, 0)),
            2 => set!(edge!(2, 5, 0, 0)),
            10 => set!(edge!(10, 5, 1, 0)),
            7 => set!(edge!(0, 7, 0, 0))
        ));
        
        assert!(data.add_component(Box::new(AND {}), vec![]).is_err());
    }
    
    #[test]
    fn test_removing_subnets() {
        let mut data = Data::new();
        
        data.add_subnet(0);
        data.add_subnet(1);
        
        assert_eq!(data.edges, map!());
        
        assert!(data.add_component(Box::new(Output {}), vec![Some(0)]).is_ok());
        
        assert_eq!(data.edges, map!(
            0 => set!(edge!(0, 3, 0, 0)),
            3 => set!(edge!(0, 3, 0, 0))
        ));
        
        assert!(data.remove_subnet(0));
        
        assert_eq!(data.edges, map!());
        
        assert!(data.remove_subnet(1));
    
        assert_eq!(data.edges, map!());
        
        assert!(!data.remove_subnet(0));
        assert!(!data.remove_subnet(3));
    }
    
    #[test]
    fn test_simulation() {
        let mut data = Data::new();
        
        data.add_subnet(0);
        data.add_subnet(1);
        
        assert!(data.add_component(Box::new(NOT {}), vec![Some(0), Some(1)]).is_ok());
        
        data.update_subnet(0, SubnetState::Off);
        
        assert_eq!(data.dirty_subnets, VecDeque::from(vec![vec![0]]));
        
        data.advance_time();
        
        assert_eq!(data.dirty_subnets, VecDeque::from(vec![vec![1]]));
        
        data.advance_time();
        
        assert_eq!(data.dirty_subnets, VecDeque::from(vec![]));
    }
}