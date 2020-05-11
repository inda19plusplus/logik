use std::collections::{HashMap, HashSet, BinaryHeap, VecDeque};
use std::collections::hash_map::Entry;

use crate::data::component::{Component, PortType};
use crate::data::subnet::{Subnet, SubnetState};
use std::cmp::Reverse;

pub(crate) mod subnet;
pub(crate) mod component;
#[cfg(test)]
mod test;

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
    dirty_subnets: VecDeque<HashSet<i32>>,
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
                //if edge.direction != EdgeDirection::ToSubnet {
                    simulating.insert((edge.component - 1) / 2);
                //}
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
            let subnet = match dirtying.get(&port) {
                Some(s) => s,
                None => continue,
            };
            self.update_subnet(*subnet, state);
        }
    }
    
    fn update_subnet(&mut self, subnet: i32, state: SubnetState) {
        self.subnets.get_mut(&subnet).unwrap().update(state);
        if self.dirty_subnets.len() == 0 { //maybe change to account for propagation time
            self.dirty_subnets.push_back(HashSet::new());
        }
        self.dirty_subnets.get_mut(0).unwrap().insert(subnet);
    }
    
    pub(crate) fn dirty_subnet(&mut self, subnet: i32) {
        self.update_subnet(subnet, self.subnets.get(&subnet).unwrap().val());
    }
    
    pub(crate) fn subnet(&self, subnet: i32) -> Option<&Subnet> {
        self.subnets.get(&subnet)
    }
    
    pub(crate) fn port_state(&self, component: i32, port: usize) -> Option<SubnetState> {
        for edge in self.edges.get(&(2 * component + 1))? {
            if edge.port == port {
                return Some(self.subnets.get(&(edge.subnet / 2))?.val())
            }
        }
        
        None
    }
    
    #[cfg(test)]
    fn update_silent(&mut self, subnet: i32, state: SubnetState) {
        self.subnets.get_mut(&subnet).unwrap().update(state);
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