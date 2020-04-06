# Interoperation
The GUI interoperates with the simulation backend by calling predefined 
library functions. 

## Simulation
The simulation backend has a macro definition `ffi` in 
[ffi.rs](native/logik_simulation/src/ffi.rs) 
which tells the compiler to use the C calling convention and to not mangle 
the function name. 

## GUI
The function is then defined in the 
[Interop/Logic.cs](src/LogikUI/Interop/Logic.cs) file. 

Add the following two lines in the `Logic` class. 
```C#
[DllImport(Lib, EntryPoint = "<sim_name>", ExactSpelling = true, CallingConvention = CallingConv)]
public static extern void <gui_name>();
```

`<sim_name>` is the name as defined in the simulation code and `<gui_name>` 
is the name of the function as it should be used in the GUI program. 
