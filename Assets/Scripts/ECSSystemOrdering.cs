using Unity.Entities;

/****** System Ordering ******

//Logic

// ShipProcessRenderingBarrier <- At this point we should be ready calculate render matrixes and render data

// UpdateShipTransfomsSystem
// RenderIndirectInstancingSystem


/*****************************/
public class ShipProcessRenderingBarrier : BarrierSystem { }