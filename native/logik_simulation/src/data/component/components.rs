use std::collections::HashMap;
use crate::data::component::{StateChange, PortType, Component};
use crate::data::subnet::SubnetState;
use crate::{map, port_or_default};

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
    
    fn evaluate(&self, _: HashMap<usize, StateChange>) -> HashMap<usize, SubnetState> {
        map!()
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
    
    fn evaluate(&self, _: HashMap<usize, StateChange>) -> HashMap<usize, SubnetState> {
        map!(0 => SubnetState::On)
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
    
    fn evaluate(&self, data: HashMap<usize, StateChange>) -> HashMap<usize, SubnetState> {
        let input = port_or_default!(data, 0);
        
        map!(1 => input)
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
    
    fn evaluate(&self, data: HashMap<usize, StateChange>) -> HashMap<usize, SubnetState> {
        let input = port_or_default!(data, 0);
        
        map!(1 => match input {
            SubnetState::Off => SubnetState::On,
            SubnetState::On => SubnetState::Off,
            _ => SubnetState::Error
        })
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
    
    fn evaluate(&self, data: HashMap<usize, StateChange>) -> HashMap<usize, SubnetState> {
        let a = port_or_default!(data, 0);
        let b = port_or_default!(data, 1);
        
        map!(
                2 =>
            if a == SubnetState::Off || b == SubnetState::Off {
                SubnetState::Off
            } else if b == SubnetState::On && b == SubnetState::On {
                SubnetState::On
            } else {
                SubnetState::Error
            }
        )
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
    
    fn evaluate(&self, data: HashMap<usize, StateChange>) -> HashMap<usize, SubnetState> {
        let a = port_or_default!(data, 0);
        let b = port_or_default!(data, 1);
        
        map!( 2 =>
            if a == SubnetState::Off || b == SubnetState::Off {
                SubnetState::On
            } else if a == SubnetState::On && b == SubnetState::On {
                SubnetState::Off
            } else {
                SubnetState::Error
            }
        )
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
    
    fn evaluate(&self, data: HashMap<usize, StateChange>) -> HashMap<usize, SubnetState> {
        let a = port_or_default!(data, 0);
        let b = port_or_default!(data, 1);
        
        
        map!(2 =>
            if a == SubnetState::On || b == SubnetState::On {
                SubnetState::On
            } else if a == SubnetState::Off && b == SubnetState::Off {
                SubnetState::Off
            } else {
                SubnetState::Error
            }
        )
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
    
    fn evaluate(&self, data: HashMap<usize, StateChange>) -> HashMap<usize, SubnetState> {
        let a = port_or_default!(data, 0);
        let b = port_or_default!(data, 1);
        
        map!(2 =>
            if a == SubnetState::On || b == SubnetState::On {
                SubnetState::Off
            } else if a == SubnetState::Off && b == SubnetState::Off {
                SubnetState::On
            } else {
                SubnetState::Error
            }
        )
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
    
    fn evaluate(&self, data: HashMap<usize, StateChange>) -> HashMap<usize, SubnetState> {
        let a = port_or_default!(data, 0);
        let b = port_or_default!(data, 1);
        
        map!(2 =>
            if a == SubnetState::Floating || a == SubnetState::Error ||
                b == SubnetState::Floating || b == SubnetState::Error {
                SubnetState::Error
            } else if a == b {
                SubnetState::Off
            } else {
                SubnetState::On
            }
        )
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
    
    fn evaluate(&self, data: HashMap<usize, StateChange>) -> HashMap<usize, SubnetState> {
        let a = port_or_default!(data, 0);
        let b = port_or_default!(data, 1);
        
        map!(2 =>
            if a == SubnetState::Floating || a == SubnetState::Error ||
                b == SubnetState::Floating || b == SubnetState::Error {
                SubnetState::Error
            } else if a == b {
                SubnetState::On
            } else {
                SubnetState::Off
            }
        )
    }
}

#[derive(Debug)]
pub(crate) struct TriBuffer {}

impl Component for TriBuffer {
    fn ports(&self) -> usize {
        3 // 0 is input, 1 is enable, 2 is output
    }
    
    fn port_type(&self, port: usize) -> Option<PortType> {
        match port {
            0 | 1 => Some(PortType::Input),
            2 => Some(PortType::Output),
            _ => None,
        }
    }
    
    fn evaluate(&self, data: HashMap<usize, StateChange>) -> HashMap<usize, SubnetState> {
        let input = port_or_default!(data, 0);
        let enable = port_or_default!(data, 1);
        
        if enable.truthy() {
            map!(2 => input)
        } else {
            map!(2 => SubnetState::Floating)
        }
    }
}

#[derive(Debug)]
pub(crate) struct TriInverter {}

impl Component for TriInverter {
    fn ports(&self) -> usize {
        3 // 0 is input, 1 is enable, 2 is output
    }
    
    fn port_type(&self, port: usize) -> Option<PortType> {
        match port {
            0 | 1 => Some(PortType::Input),
            2 => Some(PortType::Output),
            _ => None,
        }
    }
    
    fn evaluate(&self, data: HashMap<usize, StateChange>) -> HashMap<usize, SubnetState> {
        let input = port_or_default!(data, 0);
        let enable = port_or_default!(data, 1);
        
        if enable.truthy() {
            let input = match input {
                SubnetState::On => SubnetState::Off,
                SubnetState::Off => SubnetState::On,
                _ => SubnetState::Error,
            };
            map!(2 => input)
        } else {
            map!(2 => SubnetState::Floating)
        }
    }
}

#[derive(Debug)]
pub(crate) struct Probe {}

impl Component for Probe {
    fn ports(&self) -> usize {
        1
    }
    
    fn port_type(&self, port: usize) -> Option<PortType> {
        match port {
            0 => Some(PortType::Input),
            _ => None,
        }
    }
    
    fn evaluate(&self, _: HashMap<usize, StateChange>) -> HashMap<usize, SubnetState> {
        map!()
    }
}

#[derive(Debug)]
pub(crate) struct LED {}

impl Component for LED {
    fn ports(&self) -> usize {
        1
    }
    
    fn port_type(&self, port: usize) -> Option<PortType> {
        match port {
            0 => Some(PortType::Input),
            _ => None,
        }
    }
    
    fn evaluate(&self, _: HashMap<usize, StateChange>) -> HashMap<usize, SubnetState> {
        map!()
    }
}