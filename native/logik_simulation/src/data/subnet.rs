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
    
    pub(crate) fn update(&mut self, state: SubnetState) {
        self.state = state;
    }
}

/// An enum to represent the different states that a subnet can have
#[repr(u8)]
#[derive(Debug, Eq, PartialEq, Clone, Copy)]
pub enum SubnetState {
    Floating = 0,
    Off = 1,
    On = 2,
    Error = 3,
}