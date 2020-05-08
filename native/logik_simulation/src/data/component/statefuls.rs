use crate::data::component::{Component, PortType, StateChange};
use crate::data::subnet::SubnetState;
use std::collections::HashMap;
use std::cell::Cell;
use crate::map;

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
        if data.get(&2)?.rising() && data.get(&3)?.current != SubnetState::On {
            if data.get(&0)?.current == SubnetState::On &&
                (data.get(&1)?.current == SubnetState::Off ||
                    data.get(&1)?.current == SubnetState::Floating) {
                self.state.set(true);
            } else if data.get(&0)?.current == SubnetState::Off &&
                (data.get(&1)?.current == SubnetState::On ||
                    data.get(&1)?.current == SubnetState::Floating) {
                self.state.set(false);
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