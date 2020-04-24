use crate::data::subnet::Subnet;
use crate::data::component::Component;

mod subnet;
mod component;

/// Struct to represent the data that the backend should keep track of
pub struct Data {
    components: Vec<Box<dyn Component>>,
    subnets: Vec<Subnet>,
    edges: Vec<Link>,
}

/// Used to represent a link between a subnet and a component
///
/// Used in the `Data` struct.
struct Link {
    subnet: usize,
    component: usize,
}