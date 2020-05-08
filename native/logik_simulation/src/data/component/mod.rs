use std::fmt::Debug;
use crate::data::subnet::SubnetState;
use crate::map;
use std::collections::HashMap;
use crate::data::EdgeDirection;

pub(crate) mod statefuls;

/// A trait to define common behaviour between the components
pub(crate) trait Component: Debug {
    fn ports(&self) -> usize;
    fn port_type(&self, port: usize) -> Option<PortType>;
    // requires that data has a value for every input or bidirectional port
    // and in turn guarantees that the return value has a value for every output or bidirectional port
    fn evaluate(&self, data: HashMap<usize, StateChange>) -> Option<HashMap<usize, SubnetState>>;
    
    fn ports_type(&self) -> Vec<PortType> {
        (0..self.ports())
            .map(|e| self.port_type(e).unwrap())
            .collect()
    }
}

#[derive(Debug, Eq, PartialEq, Clone)]
pub(crate) enum PortType {
    Input,
    Output,
    Bidirectional,
}

impl PortType {
    pub(crate) fn to_edge_direction(&self) -> EdgeDirection {
        match self {
            PortType::Input => EdgeDirection::ToComponent,
            PortType::Bidirectional => EdgeDirection::Bidirectional,
            PortType::Output => EdgeDirection::ToSubnet,
        }
    }
}

impl From<PortType> for EdgeDirection {
    fn from(pt: PortType) -> Self {
        match pt {
            PortType::Input => EdgeDirection::ToComponent,
            PortType::Bidirectional => EdgeDirection::Bidirectional,
            PortType::Output => EdgeDirection::ToSubnet,
        }
    }
}

#[derive(Debug, Eq, PartialEq)]
pub(crate) struct StateChange {
    old: SubnetState,
    current: SubnetState,
}

impl StateChange {
    pub(crate) fn new(old: SubnetState, current: SubnetState) -> Self {
        Self { old, current }
    }
    
    pub(crate) fn rising(&self) -> bool {
        self.old == SubnetState::Off &&
            self.current == SubnetState::On
    }
    
    pub(crate) fn falling(&self) -> bool {
        self.old == SubnetState::On &&
            self.current == SubnetState::Off
    }
}

/// Placeholder for now
#[derive(Debug)]
pub(crate) struct Output {

}

impl Component for Output {
    fn ports(&self) -> usize {
        1
    }
    
    fn port_type(&self, port: usize) -> Option<PortType> {
        match port {
            0 => Some(PortType::Input),
            _ => None,
        }
    }
    
    fn evaluate(&self, _: HashMap<usize, StateChange>) -> Option<HashMap<usize, SubnetState>> {
        Some(map!())
    }
}

#[derive(Debug)]
pub(crate) struct Input {

}

impl Component for Input {
    fn ports(&self) -> usize {
        1
    }
    
    fn port_type(&self, port: usize) -> Option<PortType> {
        match port {
            0 => Some(PortType::Output),
            _ => None,
        }
    }
    
    fn evaluate(&self, _: HashMap<usize, StateChange>) -> Option<HashMap<usize, SubnetState>> {
        Some(map!(0 => SubnetState::On))
    }
}

/// Placeholder for now
#[derive(Debug)]
pub(crate) struct AND {

}

impl Component for AND {
    fn ports(&self) -> usize {
        3
    }
    
    fn port_type(&self, port: usize) -> Option<PortType> {
        match port {
            0 | 1 => Some(PortType::Input),
            2 => Some(PortType::Output),
            _ => None,
        }
    }
    
    fn evaluate(&self, data: HashMap<usize, StateChange>) -> Option<HashMap<usize, SubnetState>> {
        if !(data.contains_key(&0) && data.contains_key(&1)) {
            return None;
        }
        
        if matches!(data.get(&0).unwrap().current, SubnetState::Error | SubnetState::Floating) ||
            matches!(data.get(&1).unwrap().current, SubnetState::Error | SubnetState::Floating) {
            return Some(map!(2 => SubnetState::Error));
        }
        
        if data.get(&0).unwrap().current == SubnetState::On &&
            data.get(&1).unwrap().current == SubnetState::On {
            Some(map!(2 => SubnetState::On))
        } else {
            Some(map!(2 => SubnetState::Off))
        }
    }
}

#[derive(Debug)]
pub(crate) struct NOT {

}

impl Component for NOT {
    fn ports(&self) -> usize {
        2
    }
    
    fn port_type(&self, port: usize) -> Option<PortType> {
        match port {
            0 => Some(PortType::Input),
            1 => Some(PortType::Output),
            _ => None,
        }
    }
    
    fn evaluate(&self, data: HashMap<usize, StateChange>) -> Option<HashMap<usize, SubnetState>> {
        if !data.contains_key(&0) {
            return None;
        }
        
        Some(map!(1 => match data.get(&0).unwrap().current {
            SubnetState::Off => SubnetState::On,
            SubnetState::On => SubnetState::Off,
            _ => SubnetState::Error,
        }))
    }
}