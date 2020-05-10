use crate::data::component::{AND, Output, NOT};
use crate::{map, set};
use super::*;

macro_rules! edge {
        ($subnet:expr, $component:expr, $port:expr, 0) => {
            Edge {
                subnet: $subnet,
                component: $component,
                port: $port,
                direction: EdgeDirection::ToComponent,
            }
        };
        ($subnet:expr, $component:expr, $port:expr, 1) => {
            Edge {
                subnet: $subnet,
                component: $component,
                port: $port,
                direction: EdgeDirection::Bidirectional,
            }
        };
        ($subnet:expr, $component:expr, $port:expr, 2) => {
            Edge {
                subnet: $subnet,
                component: $component,
                port: $port,
                direction: EdgeDirection::ToSubnet,
            }
        };
    }

macro_rules! subnet {
        ($state:expr) => {
            {
                let mut s = Subnet::new();
                s.update($state);
                s
            }
        };
    }

#[test]
fn test_adding_components() {
    let mut data = Data::new();
    
    data.add_subnet(0);
    
    assert!(data.add_component(Box::new(Output {}), vec![Some(0)]).is_ok());
    
    data.add_subnet(1);
    data.add_subnet(5);
    
    assert!(data.add_component(Box::new(AND {}), vec![Some(1), Some(5), Some(0)]).is_ok());
    
    assert!(data.add_component(Box::new(Output {}), vec![Some(0)]).is_ok());
    
    assert_eq!(data.edges, map!(
            3 => set!(edge!(0, 3, 0, 0)),
            0 => set!(edge!(0, 3, 0, 0), edge!(0, 5, 2, 2), edge!(0, 7, 0, 0)),
            5 => set!(edge!(0, 5, 2, 2), edge!(2, 5, 0, 0), edge!(10, 5, 1, 0)),
            2 => set!(edge!(2, 5, 0, 0)),
            10 => set!(edge!(10, 5, 1, 0)),
            7 => set!(edge!(0, 7, 0, 0))
        ));
    
    assert!(data.add_component(Box::new(AND {}), vec![]).is_err());
}

#[test]
fn test_removing_subnets() {
    let mut data = Data::new();
    
    data.add_subnet(0);
    data.add_subnet(1);
    
    assert_eq!(data.edges, map!());
    
    assert!(data.add_component(Box::new(Output {}), vec![Some(0)]).is_ok());
    
    assert_eq!(data.edges, map!(
            0 => set!(edge!(0, 3, 0, 0)),
            3 => set!(edge!(0, 3, 0, 0))
        ));
    
    assert!(data.remove_subnet(0));
    
    assert_eq!(data.edges, map!());
    
    assert!(data.remove_subnet(1));
    
    assert_eq!(data.edges, map!());
    
    assert!(!data.remove_subnet(0));
    assert!(!data.remove_subnet(3));
}

#[test]
fn test_simulation() {
    let mut data = Data::new();
    
    data.add_subnet(0);
    data.add_subnet(1);
    
    assert!(data.add_component(Box::new(NOT {}), vec![Some(0), Some(1)]).is_ok());
    
    data.update_subnet(0, SubnetState::Off);
    
    assert_eq!(data.dirty_subnets, VecDeque::from(vec![set![0]]));
    assert_eq!(data.subnets, map!(
            0 => subnet!(SubnetState::Off),
            1 => subnet!(SubnetState::Floating)
        ));
    
    data.advance_time();
    
    assert_eq!(data.dirty_subnets, VecDeque::from(vec![set![1]]));
    assert_eq!(data.subnets, map!(
            0 => subnet!(SubnetState::Off),
            1 => subnet!(SubnetState::On)
        ));
    
    data.advance_time();
    
    assert_eq!(data.dirty_subnets, VecDeque::from(vec![]));
    assert_eq!(data.subnets, map!(
            0 => subnet!(SubnetState::Off),
            1 => subnet!(SubnetState::On)
        ));
}

#[test]
fn test_simulation_2() {
    let mut data = Data::new();
    
    data.add_subnet(1);
    data.add_subnet(2);
    data.add_subnet(5);
    data.add_subnet(7);
    
    assert!(data.add_component(Box::new(AND {}), vec![Some(1), Some(2), Some(5)]).is_ok());
    assert!(data.add_component(Box::new(NOT {}), vec![Some(7), Some(1)]).is_ok());
    
    data.update_subnet(7, SubnetState::Off);
    data.update_silent(2, SubnetState::On);
    
    assert_eq!(data.dirty_subnets, VecDeque::from(vec![set![7]]));
    assert_eq!(data.subnets, map!(
            1 => subnet!(SubnetState::Floating),
            2 => subnet!(SubnetState::On),
            5 => subnet!(SubnetState::Floating),
            7 => subnet!(SubnetState::Off)
        ));
    
    data.advance_time();
    
    assert_eq!(data.dirty_subnets, VecDeque::from(vec![set![1]]));
    assert_eq!(data.subnets, map!(
            1 => subnet!(SubnetState::On),
            2 => subnet!(SubnetState::On),
            5 => subnet!(SubnetState::Floating),
            7 => subnet!(SubnetState::Off)
        ));
    
    data.advance_time();
    
    assert_eq!(data.dirty_subnets, VecDeque::from(vec![set![5]]));
    assert_eq!(data.subnets, map!(
            1 => subnet!(SubnetState::On),
            2 => subnet!(SubnetState::On),
            5 => subnet!(SubnetState::On),
            7 => subnet!(SubnetState::Off)
        ));
    
    data.advance_time();
    
    assert_eq!(data.dirty_subnets, VecDeque::from(vec![]));
    assert_eq!(data.subnets, map!(
            1 => subnet!(SubnetState::On),
            2 => subnet!(SubnetState::On),
            5 => subnet!(SubnetState::On),
            7 => subnet!(SubnetState::Off)
        ));
}

#[test]
fn test_simulation_3() {
    let mut data = Data::new();
    
    data.add_subnet(1);
    data.add_subnet(2);
    data.add_subnet(3);
    data.add_subnet(4);
    data.add_subnet(5);
    data.add_subnet(6);
    
    assert!(data.add_component(Box::new(AND {}), vec![Some(3), Some(3), Some(4)]).is_ok());
    assert!(data.add_component(Box::new(AND {}), vec![Some(1), Some(2), Some(3)]).is_ok());
    assert!(data.add_component(Box::new(NOT {}), vec![Some(5), Some(1)]).is_ok());
    assert!(data.add_component(Box::new(NOT {}), vec![Some(6), Some(2)]).is_ok());
    
    data.update_subnet(5, SubnetState::Off);
    data.update_subnet(6, SubnetState::Off);
    
    assert_eq!(data.dirty_subnets, VecDeque::from(vec![set![5, 6]]));
    assert_eq!(data.subnets, map!(
            1 => subnet!(SubnetState::Floating),
            2 => subnet!(SubnetState::Floating),
            3 => subnet!(SubnetState::Floating),
            4 => subnet!(SubnetState::Floating),
            5 => subnet!(SubnetState::Off),
            6 => subnet!(SubnetState::Off)
        ));
    
    data.advance_time();
    
    assert_eq!(data.dirty_subnets, VecDeque::from(vec![set![1, 2]]));
    assert_eq!(data.subnets, map!(
            1 => subnet!(SubnetState::On),
            2 => subnet!(SubnetState::On),
            3 => subnet!(SubnetState::Floating),
            4 => subnet!(SubnetState::Floating),
            5 => subnet!(SubnetState::Off),
            6 => subnet!(SubnetState::Off)
        ));
    
    data.advance_time();
    
    assert_eq!(data.dirty_subnets, VecDeque::from(vec![set![3]]));
    assert_eq!(data.subnets, map!(
            1 => subnet!(SubnetState::On),
            2 => subnet!(SubnetState::On),
            3 => subnet!(SubnetState::On),
            4 => subnet!(SubnetState::Floating),
            5 => subnet!(SubnetState::Off),
            6 => subnet!(SubnetState::Off)
        ));
    
    data.advance_time();
    
    assert_eq!(data.dirty_subnets, VecDeque::from(vec![set![4]]));
    assert_eq!(data.subnets, map!(
            1 => subnet!(SubnetState::On),
            2 => subnet!(SubnetState::On),
            3 => subnet!(SubnetState::On),
            4 => subnet!(SubnetState::On),
            5 => subnet!(SubnetState::Off),
            6 => subnet!(SubnetState::Off)
        ));
}