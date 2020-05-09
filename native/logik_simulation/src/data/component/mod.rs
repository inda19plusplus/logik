use std::fmt::Debug;
use crate::data::subnet::SubnetState;
use std::collections::HashMap;
use crate::data::EdgeDirection;

pub(crate) mod statefuls;
pub(crate) mod components;

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