use crate::data::subnet::Subnet;
use crate::data::component::Component;

pub mod subnet;
pub mod component;

pub struct Data {
    components: Vec<Box<dyn Component>>,
    subnets: Vec<Subnet>,
    edges: Vec<Link>,
}

struct Link {
    subnet: usize,
    component: usize,
}