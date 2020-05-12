use std::collections::{HashMap, HashSet, BinaryHeap, VecDeque};
use std::collections::hash_map::Entry;

use crate::data::component::{Component, PortType, StateChange};
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
    clocks: Vec<i32>,
    simulation: Simulator,
}

impl Data {
    pub fn new() -> Self {
        Self {
            components: HashMap::new(),
            components_free: BinaryHeap::new(),
            subnets: HashMap::new(),
            edges: HashMap::new(),
            clocks: Vec::new(),
            simulation: Simulator::new(),
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
    
    pub(crate) fn clock(&mut self, clock_id: i32) {
        self.clocks.push(clock_id);
    }
    
    pub(crate) fn remove_component(&mut self, id: i32) -> bool {
        if self.components.remove(&id).is_none() {
            return false
        };
        
        self.components_free.push(Reverse(id));
    
        let mut to_remove = Vec::new();
    
        if let Some(edges) = self.edges.get(&id) {
            for edge in edges {
                to_remove.push(edge.clone());
            }
        }
    
        for r in to_remove {
            self.remove_edge(&r);
            self.simulation.dirty_subnet(r.subnet / 2);
        }
        
        self.simulation.process_until_clean(&self.components, &mut self.subnets, &self.edges);
        
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
        self.simulation.update_component(component, &self.components, &mut self.subnets, &self.edges);
        self.simulation.process_until_clean(&self.components, &mut self.subnets, &self.edges);
        
        true
    }
    
    pub(crate) fn unlink(&mut self, component: i32, port: usize, subnet: i32) -> bool {
        let direction = match self.port_direction_component(component, port) {
            Some(t) => t,
            None => return false,
        };
        
        self.remove_edge(&Edge::new(subnet, component, port, direction));
        self.simulation.dirty_subnet(subnet);
        self.simulation.update_component(component, &self.components, &mut self.subnets, &self.edges);
        self.simulation.process_until_clean(&self.components, &mut self.subnets, &self.edges);
        
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
    
    /// Gets the state of a subnet
    pub(crate) fn subnet_state(&self, subnet: i32) -> Option<SubnetState> {
        Some(self.subnets.get(&subnet)?.val())
    }
    
    /// Gets the state of a subnet which a port is connected to
    pub(crate) fn port_state(&self, component: i32, port: usize) -> Option<SubnetState> {
        for edge in self.edges.get(&(2 * component + 1))? {
            if edge.port == port {
                return Some(self.subnets.get(&(edge.subnet / 2))?.val())
            }
        }
        
        None
    }
    
    pub(crate) fn time_step(&mut self) {
        self.simulation.time_step(&self.clocks, &self.components, &mut self.subnets, &self.edges);
    }
}

#[cfg(test)]
impl Data {
    fn update_subnet(&mut self, subnet: i32, state: SubnetState) {
        self.simulation.update_subnet(subnet, state, &mut self.subnets);
    }
    
    fn advance_time(&mut self) {
        self.simulation.advance_time(&self.components, &mut self.subnets, &self.edges);
    }
    
    fn update_silent(&mut self, subnet: i32, state: SubnetState) {
        self.subnets.get_mut(&subnet).unwrap().update(state);
    }
}

#[derive(Debug)]
struct Simulator {
    dirty_subnets: VecDeque<HashSet<i32>>,
    changed_subnets: HashMap<i32, SubnetState>, //<subnet, old state>
}

impl Simulator {
    fn new() -> Self {
        Self {
            dirty_subnets: VecDeque::new(),
            changed_subnets: HashMap::new(),
        }
    }
    
    fn advance_time(
        &mut self,
        components: &HashMap<i32, Box<dyn Component>>,
        subnets: &mut HashMap<i32, Subnet>,
        edges: &HashMap<i32, HashSet<Edge>>
    ) {
        let to_simulate = match self.dirty_subnets.pop_front() {
            Some(t) => t,
            None => return,
        };
        
        let mut simulating = HashSet::new();
        for subnet in to_simulate {
            for edge in edges.get(&(2 * subnet)).unwrap_or(&HashSet::new()) {
                if edge.direction != EdgeDirection::ToSubnet {
                    simulating.insert((edge.component - 1) / 2);
                }
            }
        }
        
        let mut old_state = HashMap::new();
        
        std::mem::swap(&mut self.changed_subnets, &mut old_state);
        
        let mut diff: HashMap<i32, HashSet<SubnetState>> = HashMap::new();
        
        for s in simulating {
            let d = self.simulate(s, &old_state, components, subnets, edges);
            for (k, v) in d.into_iter() {
                diff.entry(k).or_default().extend(v.into_iter());
            }
        }
        
        self.apply_state_diff(diff, subnets);
    }
    
    /// Takes in a component id and the difference between the old state and the current and
    /// outputs a map of what states the subnets should have next iteration
    fn simulate(
        &mut self,
        component: i32,
        old_state: &HashMap<i32, SubnetState>,
        components: &HashMap<i32, Box<dyn Component>>,
        subnets: &HashMap<i32, Subnet>,
        edges: &HashMap<i32, HashSet<Edge>>
    ) -> HashMap<i32, HashSet<SubnetState>>
    {
        let comp = &**components.get(&component).unwrap();
        let edges = edges.get(&(2 * component + 1)).unwrap();
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
                let val = subnets.get(&(edge.subnet / 2)).unwrap().val();
                let old = match old_state.get(&(edge.subnet / 2)) {
                    Some(t) => *t,
                    None => val,
                };
                let diff = StateChange::new(old, val);
                states.insert(edge.port, diff);
            } else if dirty_ports.contains(&edge.port) {
                dirtying.insert(edge.port, edge.subnet / 2);
            }
        }
        
        let res = comp.evaluate(states);
        let mut updating: HashMap<i32, HashSet<SubnetState>> = HashMap::new();
        
        for (port, state) in res {
            if let Some(subnet) = dirtying.get(&port) {
                updating.entry(*subnet).or_default().insert(state);
            }
        }
        
        updating
    }
    
    /// Forces a component to update and advances time. Is probably called when the user places a
    /// components and wants the changes to propagate. Also empties propagates changes until no
    /// more updates are happening.
    fn update_component(
        &mut self,
        component: i32,
        components: &HashMap<i32, Box<dyn Component>>,
        subnets: &mut HashMap<i32, Subnet>,
        edges: &HashMap<i32, HashSet<Edge>>
    ) {
        let mut old_state = HashMap::new();
        
        std::mem::swap(&mut old_state, &mut self.changed_subnets);
        
        let diff = self.simulate(component, &old_state, components, subnets, edges);
        
        self.apply_state_diff(diff, subnets);
    }
    
    /// Takes a change in subnet_state and updates the relevant subnets
    fn apply_state_diff(&mut self, diff: HashMap<i32, HashSet<SubnetState>>, subnets: &mut HashMap<i32, Subnet>) {
        for (subnet, proposals) in diff {
            self.update_subnet(subnet, SubnetState::work_out_diff(&proposals), subnets);
        }
    }
    
    /// Changes a subnets value and enques it in dirty_subnets if the state changed
    fn update_subnet(&mut self, subnet: i32, state: SubnetState, subnets: &mut HashMap<i32, Subnet>) {
        let old_state = subnets.get(&subnet).unwrap().val();
        if subnets.get_mut(&subnet).unwrap().update(state) { //we actually changed a subnet
            if self.dirty_subnets.len() == 0 { //maybe change to account for propagation time
                self.dirty_subnets.push_back(HashSet::new());
            }
            self.dirty_subnets.get_mut(0).unwrap().insert(subnet);
            self.changed_subnets.insert(subnet, old_state);
        }
    }
    
    /// Dirties a subnet
    fn dirty_subnet(&mut self, subnet: i32) {
        if self.dirty_subnets.len() == 0 {
            self.dirty_subnets.push_back(HashSet::new());
        }
        self.dirty_subnets.get_mut(0).unwrap().insert(subnet);
    }
    
    fn process_until_clean(
        &mut self,
        components: &HashMap<i32, Box<dyn Component>>,
        subnets: &mut HashMap<i32, Subnet>,
        edges: &HashMap<i32, HashSet<Edge>>
    ) -> bool {
        const MAX_ITERS: i32 = 1000;
        for _ in 0..MAX_ITERS {
            self.advance_time(components, subnets, edges);
        }
        
        false
    }
    
    /// Toggles all clocks
    fn time_step(
        &mut self,
        clocks: &[i32],
        components: &HashMap<i32, Box<dyn Component>>,
        subnets: &mut HashMap<i32, Subnet>,
        edges: &HashMap<i32, HashSet<Edge>>
    ) {
        let mut updating = Vec::new();
        for clock in clocks {
            updating.push(*clock);
        }
        
        for u in updating {
            self.update_component(u, components, subnets, edges);
        }
        
        self.process_until_clean(components, subnets, edges);
    }
}

/// Stores subnets and components in edge-query format
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