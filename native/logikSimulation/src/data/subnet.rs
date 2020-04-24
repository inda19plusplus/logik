
pub struct Subnet {
    state: SubnetState,
}

pub enum SubnetState {
    Off,
    On,
    Floating,
    Error,
}