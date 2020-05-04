pub mod data;
#[macro_use]
mod ffi;

use std::ffi::CStr;
use std::os::raw::c_char;

#[repr(C)]
pub struct CoolStruct {
    id: i32,
    name: *const c_char,
}

ffi! {
    fn test() {
        println!("Hello word from rust!");
    }

    fn test2(val: i32) {
        println!("Hello word from rust! And an int from C#: {}!", val);
    }

    fn add(val1: i32, val2: i32) -> i32 {
        val1 + val2
    }

    fn do_cool_stuff(arg : &CoolStruct) {
        let a = CStr::from_ptr(arg.name);
        let b = a.to_string_lossy();
        let c = b.into_owned();
        println!("{}: {}", (*arg).id, c);
    }
}

/*
ffi! {
    fn init() {
        // Initialization code
    }

    fn exit() {
        // Exit code
    }
}*/
