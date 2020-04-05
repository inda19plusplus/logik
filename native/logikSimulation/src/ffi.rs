#[macro_use]
macro_rules! ffi {
    ($(fn $name:ident ( $( $arg_ident:ident : $arg_ty:ty),* ) $( -> $ret_ty:ty)? $body:block)*) => {
        $(
            #[allow(unsafe_code, unused_attributes)]
            #[no_mangle]
            pub unsafe extern "cdecl" fn $name( $($arg_ident : $arg_ty),* ) $(-> $ret_ty)? {
                $body
            }
        )*
    };
}