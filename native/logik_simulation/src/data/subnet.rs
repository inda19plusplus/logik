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
}

/// An enum to represent the different states that a subnet can have
#[derive(Debug)]
pub(crate) enum SubnetState {
    Off,
    On,
    Floating,
    Error,
}