use std::collections::HashSet;

/// Representing a subnet with a certain state
#[derive(Debug, Eq, PartialEq)]
pub(crate) struct Subnet {
    state: SubnetState,
}

impl Subnet {
    pub(crate) fn new() -> Self {
        Self {
            state: SubnetState::Floating,
        }
    }
    
    pub(crate) fn val(&self) -> SubnetState {
        self.state
    }
    
    pub(crate) fn update(&mut self, state: SubnetState) -> bool {
        if self.state == state {
            false
        } else {
            self.state = state;
            true
        }
    }
}

/// An enum to represent the different states that a subnet can have
#[repr(u8)]
#[derive(Debug, Eq, PartialEq, Clone, Copy, Hash)]
pub enum SubnetState {
    Floating = 0,
    Off = 1,
    On = 2,
    Error = 3,
}

impl SubnetState {
    pub(crate) fn work_out_diff(diff: &HashSet<SubnetState>) -> SubnetState {
        let filtered = diff.into_iter()
            .filter(|e| e != &&SubnetState::Floating)
            .collect::<Vec<_>>();
        
        match filtered.len() {
            0 => SubnetState::Floating,
            1 => *filtered[0],
            _ => SubnetState::Error,
        }
    }
}