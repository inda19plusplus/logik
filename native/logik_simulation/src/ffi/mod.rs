use crate::data::Data;
use std::iter;
use crate::data::subnet::SubnetState;
use crate::data::component::components::*;
use crate::data::component::Component;
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
pub extern "C" fn add_component(data: *mut Data, component: i32) -> i32 {
    let data = unsafe { &mut *data};
    
    let component: Box<dyn Component> = match component {
        2 => Box::new(OutputGate {}),
        3 => Box::new(InputGate {}),
        50 => Box::new(Buffer {}),
        51 => Box::new(NOT {}),
        52 => Box::new(AND {}),
        53 => Box::new(NAND {}),
        54 => Box::new(OR {}),
        55 => Box::new(NOR {}),
        56 => Box::new(XOR {}),
        57 => Box::new(XNOR {}),
        60 => Box::new(TriBuffer {}),
        61 => Box::new(TriInverter {}),
        100 => Box::new(DFlipFlop::new()),
        101 => Box::new(TFlipFlop::new()),
        102 => Box::new(JKFlipFlop::new()),
        103 => Box::new(SRFlipFlop::new()),
        _ => unreachable!()
    };
    
    let p = component.ports();
    
    let res = data.add_component(component, iter::repeat(None).take(p).collect());
    
    res.unwrap() as i32
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
    
    data.advance_time();
}

#[no_mangle]
pub extern "C" fn dirty_subnet(data: *mut Data, subnet: i32) {
    let data = unsafe { &mut *data };
    
    data.dirty_subnet(subnet);
}

#[no_mangle]
pub extern "C" fn subnet_state(data: *mut Data, subnet: i32) -> SubnetState {
    let data = unsafe { &mut *data };
    
    data.subnet(subnet).unwrap().val()
}

#[no_mangle]
pub extern "C" fn port_state(data: *mut Data, component: i32, port: i32) -> SubnetState {
    let data = unsafe { &mut *data };
    
    data.port_state(component, port as usize).unwrap()
}