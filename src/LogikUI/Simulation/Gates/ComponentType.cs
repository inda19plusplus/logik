namespace LogikUI.Simulation.Gates
{
    enum ComponentType : int
    {
        // If the component type is custom it's a user defined component
        // and requires additional data to operate on...
        Custom    = 0,

        // IO components (reserved range 1-49)
        Constant    = 1,
        Output      = 2,
        Input       = 3,
        InputOutput = 4,
        // LED = 5,
        // LEDMatrix = 6,
        // SevenSegment = 7,
        // Button = 8,
        // Switch = 9,

        // Normal/standard gates (reserved range 50-99)
        Buffer = 50,
        Not    = 51,
        And    = 52,
        Nand   = 53,
        Or     = 54,
        Nor    = 55,
        Xor    = 56,
        Xnor   = 57,
        //Imply  = 59,
        //Nimply  = 59,

        TriStateBuffer   = 60,
        TriStateInverter = 61,

        // More advanced/complex components (reserved range 100-299)
        DFlipFlop     = 100,
        TFlipFlop     = 101,
        JKFlipFlop    = 102,
        SRFlipFlop    = 103,
        Register      = 104,
        ShiftRegister = 105,
        Counter       = 106,

        RAM        = 110,
        ROM        = 111,

        // Figure out different sizes...
        // Mux       = xxx,
        // DeMux     = xxx,
        // 

        // Utility (reserved range 300-399)
        Probe = 300,
        Splitter = 301,
        Clock    = 302,
        // Tunnel   = 303,
    }
}
