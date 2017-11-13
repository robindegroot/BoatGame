using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BoatPhysics : MonoBehaviour {
    public GameObject underWaterObject;
    private ModifyBoatMesh modifyBoatMesh;
    private Mesh underWaterMesh;
    private Rigidbody boat;
    private float rhoWater = 480f;
	
	void Start ()
    {
        boat = gameObject.GetComponent<Rigidbody>();
        modifyBoatMesh = new ModifyBoatMesh(gameObject);
        underWaterMesh = underWaterObject.GetComponent<MeshFilter>().mesh;	
	}
	
	
	void Update ()
    {
        modifyBoatMesh.GenerateUnderwaterMesh();
        modifyBoatMesh.DisplayMesh(underWaterMesh, "Underwater Mesh", modifyBoatMesh.underWaterTriangleData);	
	}
    void FixedUpdate()
    {
        if(modifyBoatMesh.underWaterTriangleData.Count > 0)
        {
            AddUnderWaterForces();
        }
    }
    void AddUnderWaterForces()
    {
        List<TriangleData> underWaterTriangleData = modifyBoatMesh.underWaterTriangleData;
        for(int i = 0; i < underWaterTriangleData.Count; i++)
        {
            TriangleData triangleData = underWaterTriangleData[i];
            Vector3 buoyancyForce = BuoyancyForce(rhoWater, triangleData);
            boat.AddForceAtPosition(buoyancyForce, triangleData.center);
            Debug.DrawRay(triangleData.center, triangleData.normal * 3f, Color.white);
            Debug.DrawRay(triangleData.center, buoyancyForce.normalized * -3f, Color.blue);
        }
    }
    private Vector3 BuoyancyForce(float rho, TriangleData triangleData)
    {
        //Buoyancy is a hydrostatic force - it's there even if the water isn't flowing or if the boat stays still

        // F_buoyancy = rho * g * V
        // rho - density of the mediaum you are in
        // g - gravity
        // V - volume of fluid directly above the curved surface 

        // V = z * S * n 
        // z - distance to surface
        // S - surface area
        // n - normal to the surface
        Vector3 buoyancyForce = rho * Physics.gravity.y * triangleData.distanceToSurface * triangleData.area * triangleData.normal;

        buoyancyForce.x = 0f;
        buoyancyForce.z = 0f;

        return buoyancyForce;
    }
}
