use std::collections::HashMap;
use crate::data::component::{StateChange, PortType, Component};
use crate::data::subnet::SubnetState;
use crate::map;

/// Placeholder for now
#[derive(Debug)]
pub(crate) struct OutputGate {}

impl Component for OutputGate {
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
pub(crate) struct InputGate {}

impl Component for InputGate {
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

#[derive(Debug)]
pub(crate) struct Buffer {}

impl Component for Buffer {
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
        
        Some(map!(1 => data.get(&0).unwrap().current))
    }
}

#[derive(Debug)]
pub(crate) struct NOT {}

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
            SubnetState::Floating => SubnetState::Floating,
            SubnetState::Error => SubnetState::Error
        }))
    }
}

/// Placeholder for now
#[derive(Debug)]
pub(crate) struct AND {}

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
        
        if let (Some(port1), Some(port2)) = (data.get(&0), data.get(&1)) {
            Some(map!(
                2 =>
            if port1.current == SubnetState::Off || port2.current == SubnetState::Off {
                SubnetState::Off
            } else if port1.current == SubnetState::On && port2.current == SubnetState::On {
                SubnetState::On
            } else {
                SubnetState::Floating
            }))
        } else {
            None
        }
    }
}

#[derive(Debug)]
pub(crate) struct NAND {}

impl Component for NAND {
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
        
        if let (Some(port1), Some(port2)) = (data.get(&0), data.get(&1)) {
            Some(map!(
                2 =>
            if port1.current == SubnetState::Off || port2.current == SubnetState::Off {
                SubnetState::On
            } else if port1.current == SubnetState::On && port2.current == SubnetState::On {
                SubnetState::Off
            } else {
                SubnetState::Floating
            }))
        } else {
            None
        }
    }
}

#[derive(Debug)]
pub(crate) struct OR {}

impl Component for OR {
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
        
        if let (Some(port1), Some(port2)) = (data.get(&0), data.get(&1)) {
            Some(map!(
                2 =>
            if port1.current == SubnetState::On || port2.current == SubnetState::On {
                SubnetState::On
            } else if port1.current == SubnetState::Off && port2.current == SubnetState::Off {
                SubnetState::Off
            } else {
                SubnetState::Floating
            }))
        } else {
            None
        }
    }
}

#[derive(Debug)]
pub(crate) struct NOR {}

impl Component for NOR {
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
        
        if let (Some(port1), Some(port2)) = (data.get(&0), data.get(&1)) {
            Some(map!(
                2 =>
            if port1.current == SubnetState::On || port2.current == SubnetState::On {
                SubnetState::Off
            } else if port1.current == SubnetState::Off && port2.current == SubnetState::Off {
                SubnetState::On
            } else {
                SubnetState::Floating
            }))
        } else {
            None
        }
    }
}

#[derive(Debug)]
pub(crate) struct XOR {}

impl Component for XOR {
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
        
        if let (Some(port1), Some(port2)) = (data.get(&0), data.get(&1)) {
            Some(map!(
                2 =>
            if port1.current == SubnetState::Floating || port1.current == SubnetState::Error ||
                port2.current == SubnetState::Floating || port2.current == SubnetState::Error {
                SubnetState::Floating
            } else if port1.current == port2.current {
                SubnetState::Off
            } else {
                SubnetState::On
            }))
        } else {
            None
        }
    }
}

#[derive(Debug)]
pub(crate) struct XNOR {}

impl Component for XNOR {
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
        
        if let (Some(port1), Some(port2)) = (data.get(&0), data.get(&1)) {
            Some(map!(
                2 =>
            if port1.current == SubnetState::Floating || port1.current == SubnetState::Error ||
                port2.current == SubnetState::Floating || port2.current == SubnetState::Error {
                SubnetState::Floating
            } else if port1.current == port2.current {
                SubnetState::On
            } else {
                SubnetState::Off
            }))
        } else {
            None
        }
    }
}