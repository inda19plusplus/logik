use crate::data::component::{Component, PortType, StateChange};
use crate::data::subnet::SubnetState;
use std::collections::HashMap;
use std::cell::Cell;
use crate::map;

impl SRFlipFlop {
    pub(crate) fn new() -> Self {
        Self { state: Cell::new(false) }
    }
}

#[derive(Debug)]
pub(crate) struct DFlipFlop {
    #[cfg(not(test))]
    state: Cell<bool>,
    #[cfg(test)]
    pub state: Cell<bool>,
}

impl Component for DFlipFlop {
    fn ports(&self) -> usize {
        5 // 0 is D, 1 is clock, 2 is disable, 3 is Q, 4 is Q inverse
    }
    
    fn port_type(&self, port: usize) -> Option<PortType> {
        match port {
            0..=2 => Some(PortType::Input),
            3 | 4 => Some(PortType::Output),
            _ => None,
        }
    }
    
    fn evaluate(&self, data: HashMap<usize, StateChange>) -> Option<HashMap<usize, SubnetState>> {
        if let (Some(port0), Some(port1), Some(port2)) =
            (data.get(&0), data.get(&1), data.get(&2)) {
            if port1.rising() && port2.current != SubnetState::On {
                if port0.current == SubnetState::On {
                    self.state.set(true);
                } else if port0.current == SubnetState::Off {
                    self.state.set(false);
                }
            }
        }
    
        let vals = match self.state.get() {
            true => (SubnetState::On, SubnetState::Off),
            false => (SubnetState::Off, SubnetState::On),
        };
    
        Some(map!(
                3 => vals.0,
                4 => vals.1
            ))
    }
}

impl DFlipFlop {
    pub(crate) fn new() -> Self {
        Self { state: Cell::new(false) }
    }
}

#[derive(Debug)]
pub(crate) struct TFlipFlop {
    #[cfg(not(test))]
    state: Cell<bool>,
    #[cfg(test)]
    pub state: Cell<bool>,
}

impl Component for TFlipFlop {
    fn ports(&self) -> usize {
        5 // 0 is T, 1 is clock, 2 is disable, 3 is Q, 4 is Q inverse
    }
    
    fn port_type(&self, port: usize) -> Option<PortType> {
        match port {
            0..=2 => Some(PortType::Input),
            3 | 4 => Some(PortType::Output),
            _ => None,
        }
    }
    
    fn evaluate(&self, data: HashMap<usize, StateChange>) -> Option<HashMap<usize, SubnetState>> {
        if let (Some(port0), Some(port1), Some(port2)) =
        (data.get(&0), data.get(&1), data.get(&2)) {
            if port1.rising() && port2.current != SubnetState::On {
                if port0.current.truthy() {
                    self.state.set(!self.state.get());
                }
            }
        }
        
        let vals = match self.state.get() {
            true => (SubnetState::On, SubnetState::Off),
            false => (SubnetState::Off, SubnetState::On),
        };
        
        Some(map!(
                3 => vals.0,
                4 => vals.1
            ))
    }
}

impl TFlipFlop {
    pub(crate) fn new() -> Self {
        Self { state: Cell::new(false) }
    }
}

#[derive(Debug)]
pub(crate) struct JKFlipFlop {
    #[cfg(not(test))]
    state: Cell<bool>,
    #[cfg(test)]
    pub state: Cell<bool>,
}

impl Component for JKFlipFlop {
    fn ports(&self) -> usize {
        6 // 0 is J, 1 is K, 2 is clock, 3 is disable, 4 is Q, 5 is Q inverse
    }
    
    fn port_type(&self, port: usize) -> Option<PortType> {
        match port {
            0..=3 => Some(PortType::Input),
            4 | 5 => Some(PortType::Output),
            _ => None,
        }
    }
    
    fn evaluate(&self, data: HashMap<usize, StateChange>) -> Option<HashMap<usize, SubnetState>> {
        if let (Some(port0), Some(port1), Some(port2), Some(port3)) =
            (data.get(&0), data.get(&1), data.get(&2), data.get(&3)) {
            if port2.rising() && port3.current != SubnetState::On {
                if port0.current.truthy() && port1.current.falsy() {
                    self.state.set(true);
                } else if port0.current.falsy() && port1.current.truthy() {
                    self.state.set(false);
                } else if port0.current.truthy() && port1.current.truthy() {
                    self.state.set(!self.state.get());
                }
            }
        }
        
        let vals = match self.state.get() {
            true => (SubnetState::On, SubnetState::Off),
            false => (SubnetState::Off, SubnetState::On),
        };
        
        Some(map!(
            4 => vals.0,
            5 => vals.1
        ))
    }
}

impl JKFlipFlop {
    pub(crate) fn new() -> Self {
        Self { state: Cell::new(false) }
    }
}

#[derive(Debug)]
pub(crate) struct SRFlipFlop {
    #[cfg(not(test))]
    state: Cell<bool>,
    #[cfg(test)]
    pub state: Cell<bool>,
}

impl Component for SRFlipFlop {
    fn ports(&self) -> usize {
        6 // 0 is S, 1 is R, 2 is clock, 3 is disable, 4 is Q, 5 is Q inverse
    }
    
    fn port_type(&self, port: usize) -> Option<PortType> {
        match port {
            0..=3 => Some(PortType::Input),
            4 | 5 => Some(PortType::Output),
            _ => None,
        }
    }
    
    fn evaluate(&self, data: HashMap<usize, StateChange>) -> Option<HashMap<usize, SubnetState>> {
        if let (Some(port0), Some(port1), Some(port2), Some(port3)) =
            (data.get(&0), data.get(&1), data.get(&2), data.get(&3)) {
            if port2.rising() && port3.current != SubnetState::On {
                if port0.current.truthy() && port1.current.falsy() {
                    self.state.set(true);
                } else if port1.current.truthy() && port0.current.falsy() {
                    self.state.set(false);
                }
            }
        }
        
        let vals = match self.state.get() {
            true => (SubnetState::On, SubnetState::Off),
            false => (SubnetState::Off, SubnetState::On),
        };
        
        Some(map!(
            4 => vals.0,
            5 => vals.1
        ))
    }
}