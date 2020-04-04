
#[no_mangle]
pub unsafe extern "C" fn main2(val: i32) {
    println!("Hello word from rust! And an int from C#: {}!", val);
}
