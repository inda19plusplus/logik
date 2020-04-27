/// Representing a subnet with a certain state
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
pub(crate) enum SubnetState {
    Off,
    On,
    Floating,
    Error,
}