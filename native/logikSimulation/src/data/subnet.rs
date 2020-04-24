/// Representing a subnet with a certain state
pub(super) struct Subnet {
    state: SubnetState,
}

/// An enum to represent the different states that a subnet can have
pub(super) enum SubnetState {
    Off,
    On,
    Floating,
    Error,
}