use std::fmt::Debug;

/// A trait to define common behaviour between the components
pub(crate) trait Component: Debug {
    fn ports(&self) -> usize;
    fn port_type(&self, port: usize) -> Option<PortType>;
}

pub(crate) enum PortType {
    Input,
    Output,
    Bidirectional,
}

/// Placeholder for now
#[derive(Debug)]
pub(crate) struct Output {

}

impl Component for Output {
    fn ports(&self) -> usize {
        1
    }
    
    fn port_type(&self, port: usize) -> Option<PortType> {
        match port {
            0 => Some(PortType::Input),
            _ => None,
        }
    }
}

/// Placeholder for now
#[derive(Debug)]
pub(crate) struct AND {

}

impl Component for AND {
    fn ports(&self) -> usize {
        3
    }
    
    fn port_type(&self, port: usize) -> Option<PortType> {
        match port {
            0 | 1 => Some(PortType::Input),
            2 => Some(PortType::Output),
            _ => None,
        }
    }
}