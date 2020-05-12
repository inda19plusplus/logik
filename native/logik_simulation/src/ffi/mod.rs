use crate::data::Data;
use std::iter;
use crate::data::subnet::SubnetState;
use crate::data::component::components::*;
use crate::data::component::{Component, ComponentId};
use crate::data::component::statefuls::*;

#[cfg(test)]
mod test;

#[no_mangle]
pub extern "C" fn init() -> *mut Data {
    Box::into_raw(Box::new(Data::new()))
}

#[no_mangle]
pub extern "C" fn exit(data: *mut Data) {
    unsafe { drop(Box::from_raw(data)) };
}

#[no_mangle]
pub extern "C" fn add_subnet(data: *mut Data, id: i32) -> bool {
    let data = unsafe { &mut *data};
    
    data.add_subnet(id)
}

#[no_mangle]
pub extern "C" fn remove_subnet(data: *mut Data, id: i32) -> bool {
    let data = unsafe { &mut *data};
    
    data.remove_subnet(id)
}

#[no_mangle]
pub extern "C" fn add_component(data: *mut Data, component: ComponentId) -> i32 {
    let data = unsafe { &mut *data};
    
    let comp: Box<dyn Component> = match component {
        ComponentId::Constant => Box::new(Constant::new()),
        ComponentId::Output => Box::new(OutputGate {}),
        ComponentId::Input => Box::new(InputGate {}),
        ComponentId::LED => Box::new(LED {}),
        ComponentId::Button => Box::new(Button::new()),
        ComponentId::Buffer => Box::new(Buffer {}),
        ComponentId::Not => Box::new(NOT {}),
        ComponentId::And => Box::new(AND {}),
        ComponentId::Nand => Box::new(NAND {}),
        ComponentId::Or => Box::new(OR {}),
        ComponentId::Nor => Box::new(NOR {}),
        ComponentId::Xor => Box::new(XOR {}),
        ComponentId::Xnor => Box::new(XNOR {}),
        ComponentId::TriStateBuffer => Box::new(TriBuffer {}),
        ComponentId::TriStateInverter => Box::new(TriInverter {}),
        ComponentId::DFlipFlop => Box::new(DFlipFlop::new()),
        ComponentId::TFlipFlop => Box::new(TFlipFlop::new()),
        ComponentId::JKFlipFlop => Box::new(JKFlipFlop::new()),
        ComponentId::SRFlipFlop => Box::new(SRFlipFlop::new()),
        ComponentId::Probe => Box::new(Probe {}),
        ComponentId::Clock => Box::new(Clock::new()),
    };
    
    let p = comp.ports();
    
    let res = data.add_component(comp, iter::repeat(None).take(p).collect()).unwrap();
    
    if component == ComponentId::Clock {
        data.clock(res);
    }
    
    res
}

#[no_mangle]
pub extern "C" fn remove_component(data: *mut Data, id: i32) -> bool {
    let data = unsafe { &mut *data };
    
    data.remove_component(id)
}

#[no_mangle]
pub extern "C" fn link(data: *mut Data, component: i32, port: i32, subnet: i32) -> bool {
    let data = unsafe { &mut *data };
    
    data.link(component, port as usize, subnet)
}

#[no_mangle]
pub extern "C" fn unlink(data: *mut Data, component: i32, port: i32, subnet: i32) -> bool {
    let data = unsafe { &mut *data };
    
    data.unlink(component, port as usize, subnet)
}

#[no_mangle]
pub extern "C" fn tick(data: *mut Data) {
    let data = unsafe { &mut *data };
    
    data.time_step();
}

#[no_mangle]
pub extern "C" fn subnet_state(data: *mut Data, subnet: i32) -> SubnetState {
    let data = unsafe { &mut *data };
    
    data.subnet_state(subnet).unwrap()
}

#[no_mangle]
pub extern "C" fn port_state(data: *mut Data, component: i32, port: i32) -> SubnetState {
    let data = unsafe { &mut *data };
    
    match data.port_state(component, port as usize) {
        Some(state) => state,
        None => SubnetState::Floating,
	}
}