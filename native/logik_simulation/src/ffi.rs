use crate::data::Data;
use crate::data::component::{Component, Input, AND};
use std::iter;
use crate::data::subnet::SubnetState;
// Converts a rust function definition into one that can be called from c (and by extension c#).
// A function with the signature 'fn <name>(<params>) <return> { <body> }'
/* Into a function with the signature 

#[allow(unsafe_code, unused_attributes)]
#[no_mangle]
pub unsafe extern "cdecl" fn <name>(<params>) <return> {
    <body>
}

*/

/*macro_rules! ffi {
    ($(fn $name:ident ( $( $arg_ident:ident : $arg_ty:ty),* ) $( -> $ret_ty:ty)? $body:block)*) => {
        $(
            #[allow(unsafe_code, unused_attributes)]
            #[no_mangle]
            pub unsafe extern "cdecl" fn $name( $($arg_ident : $arg_ty),* ) $(-> $ret_ty)? {
                $body
            }
        )*
    };
}*/

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
        1 => Box::new(Input {}),
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
    
    // FIXME: This is a temporary fix and should not be checked in
    match data.port_state(component, port as usize) {
        Some(state) => state,
        None => SubnetState::Floating,
    }
}