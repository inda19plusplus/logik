use crate::data::Data;
use crate::data::component::{Component, Output, AND};
use std::iter;
use crate::data::subnet::SubnetState;

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
        0 => Box::new(Output {}),
        _ => Box::new(AND {}) // TODO: make me the same as the ID:s in C#
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