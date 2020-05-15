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
    fn evaluate(&self, data: HashMap<usize, StateChange>) -> HashMap<usize, SubnetState>;
    
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

#[derive(Debug, Eq, PartialEq, Clone, Copy)]
#[repr(i32)]
pub enum ComponentId {
    Constant = 1,
    Output = 2,
    Input = 3,
    LED = 5,
    Button = 8,
    Switch = 9,
    Buffer = 50,
    Not = 51,
    And = 52,
    Nand = 53,
    Or = 54,
    Nor = 55,
    Xor = 56,
    Xnor = 57,
    TriStateBuffer   = 60,
    TriStateInverter = 61,
    DFlipFlop     = 100,
    TFlipFlop     = 101,
    JKFlipFlop    = 102,
    SRFlipFlop    = 103,
    Probe = 300,
    Clock    = 302,
}

#[macro_export]
macro_rules! port_or_default {
    ($data:ident ,$id:expr) => {
        $data.get(&($id)).map(|e| e.current).unwrap_or(SubnetState::Floating)
    };
}