use std::collections::HashSet;

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
    
    pub(crate) fn update(&mut self, state: SubnetState) -> bool {
        if self.state == state {
            false
        } else {
            self.state = state;
            true
        }
    }
}

/// An enum to represent the different states that a subnet can have
#[repr(u8)]
#[derive(Debug, Eq, PartialEq, Clone, Copy, Hash)]
pub enum SubnetState {
    Floating = 0,
    Off = 1,
    On = 2,
    Error = 3,
}

impl SubnetState {
    pub(crate) fn work_out_diff(diff: &HashSet<SubnetState>) -> SubnetState {
        let filtered = diff.into_iter()
            .filter(|e| e != &&SubnetState::Floating)
            .collect::<Vec<_>>();
        
        match filtered.len() {
            0 => SubnetState::Floating,
            1 => *filtered[0],
            _ => SubnetState::Error,
        }
    }
    
    pub(crate) fn truthy(&self) -> bool {
        match self {
            SubnetState::On => true,
            _ => false,
        }
    }
    
    pub(crate) fn falsy(&self) -> bool {
        match self {
            SubnetState::Off | SubnetState::Floating => true,
            _ => false,
        }
    }
}

#[cfg(test)]
mod test {
    use crate::data::subnet::SubnetState;
    use crate::set;
    
    #[test]
    fn test_working_out_subnet_diff() {
        let d1 = set!(SubnetState::Floating, SubnetState::On);
        assert_eq!(SubnetState::work_out_diff(&d1), SubnetState::On);
        
        let d2 = set!(SubnetState::Floating, SubnetState::Floating);
        assert_eq!(SubnetState::work_out_diff(&d2), SubnetState::Floating);
        
        let d3 = set!(SubnetState::On, SubnetState::Off, SubnetState::Floating);
        assert_eq!(SubnetState::work_out_diff(&d3), SubnetState::Error);
        
        let d4 = set!(SubnetState::On, SubnetState::On);
        assert_eq!(SubnetState::work_out_diff(&d4), SubnetState::On);
        
        let d5 = set!(SubnetState::Off, SubnetState::Off, SubnetState::Floating);
        assert_eq!(SubnetState::work_out_diff(&d5), SubnetState::Off);
        
        let d6 = set!(SubnetState::Off, SubnetState::On);
        assert_eq!(SubnetState::work_out_diff(&d6), SubnetState::Error);
    }
}