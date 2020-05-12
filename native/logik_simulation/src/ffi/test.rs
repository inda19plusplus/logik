use super::*;

#[test]
fn test_unlinking_unlinked() {
    let mut data = init();
    
    assert!(add_subnet(data, 0));
    let comp = add_component(data, ComponentId::Buffer);
    
    assert!(!unlink(data, comp, 0, 0));
    
    exit(data);
}