
#[macro_export]
macro_rules! map (
    ( $($key:expr => $value:expr),+ ) => {
        {
            let mut m = ::std::collections::HashMap::new();
            $(
                m.insert($key, $value);
            )+
            m
        }
    };
    () => {
        {
            ::std::collections::HashMap::new()
        }
    };
);

#[macro_export]
macro_rules! set (
    ( $($val:expr),+ ) => {
        {
            let mut s = ::std::collections::HashSet::new();
            $(
                s.insert($val);
            )+
            s
        }
    };
    () => {
        {
            ::std::collections::HashSet::new();
        }
    };
);