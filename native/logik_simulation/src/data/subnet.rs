/// Representing a subnet with a certain state
#[derive(Debug)]
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
#[derive(Debug, Eq, PartialEq, Clone, Copy)]
pub(crate) enum SubnetState {
    Off,
    On,
    Floating,
    Error,
}