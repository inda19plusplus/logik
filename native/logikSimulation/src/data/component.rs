/// A trait to define common behaviour between the components
pub(crate) trait Component {
    fn inputs(&self) -> usize;
    fn outputs(&self) -> usize;
}

/// Placeholder for now
pub(crate) struct Output {

}

impl Component for Output {
    fn inputs(&self) -> usize {
        1
    }
    
    fn outputs(&self) -> usize {
        0
    }
}

/// Placeholder for now
pub(crate) struct AND {

}

impl Component for AND {
    fn inputs(&self) -> usize {
        2
    }
    
    fn outputs(&self) -> usize {
        1
    }
}