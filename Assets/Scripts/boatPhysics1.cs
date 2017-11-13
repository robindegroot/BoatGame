using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class boatPhysics1 : MonoBehaviour {
    public GameObject boatMeshObj;
    public GameObject underWaterObj;
    public GameObject aboveWaterObj;
    public Vector3 centerOfMass;
    private ModifyBoatMesh modifyBoatMesh;
    private Mesh underWaterMesh;
    private Mesh aboveWaterMesh;
    private Rigidbody boat;
    private float water = BoatPhysicsMath.OCEAN_WATER;
    private float air = BoatPhysicsMath.AIR;

    private void Awake()
    {
        boat = this.GetComponent<Rigidbody>();
    }
    private void Start()
    {
        modifyBoatMesh = new ModifyBoatMesh(boatMeshObj, underWaterObj, aboveWaterObj, boat);
        underWaterMesh = underWaterObj.GetComponent<MeshFilter>().mesh;
        aboveWaterMesh = aboveWaterObj.GetComponent<MeshFilter>().mesh;
    }
    private void Update()
    {
        modifyBoatMesh.GenerateUnderwaterMesh();
        modifyBoatMesh.DisplayMesh(underWaterMesh, "UnderWater Mesh", modifyBoatMesh.underWaterTriangleData);
        //modifyBoatMesh.DisplayMesh(aboveWaterMesh, "AboveWater Mesh", modifyBoatMesh.aboveWaterTriangleData);
    }
    private void FixedUpdate()
    {
        boat.centerOfMass = centerOfMass;
        if(modifyBoatMesh.underWaterTriangleData.Count > 0)
        {
            AddUnderWaterForces();
        }
        if(modifyBoatMesh.aboveWaterTriangleData.Count > 0)
        {
            AddAboveWaterForces();
        }
    }
    void AddUnderWaterForces()
    {
        float Cf = BoatPhysicsMath.ResistanceCoefficient(
            water,
            boat.velocity.magnitude,
            modifyBoatMesh.CalculateUnderWaterLength());
        List<SlammingForceData> slammingForceData = modifyBoatMesh.slammingForceData;
        CalculateSlammingVelocities(slammingForceData);

        float boatArea = modifyBoatMesh.boatArea;
        float boatMass = VisbyData.mass;
        List<int> indexOfOriginalTriangle = modifyBoatMesh.indexOfOriginalTriangle;

        for (int i = 0; i < underWaterTriangleData.Count; i++)
        {
            TriangleData triangleData = underWaterTriangleData[i];

            vector3 forceToAdd = Vector3.zero;
            forceToAdd += BoatPhysicsMath.BuoyancyForce(water, triangleData);
            forceToAdd += BoatPhysicsMath.ViscousWaterResistanceForce(water, triangleData, Cf);
            forceToAdd += BoatPhysicsMath.PressureDragForce(triangleData);
            int originalTriangleIndex = indexOfOriginalTriangle[i];

            slammingForceData slammingData = slammingForceData[originalTriangleIndex];

            forceToAdd += boatPhysicsMath.SlammingForce(slammingData, triangleData, boatArea, boatMass);

            boatRB.addForceAtPosition(forceToAdd, triangleData.center);

        }

    }
    private void CalculateSlammingVelocities(List<SlammingForceData> slammingForceData)
    {
        for (int i = 0; i < slammingForceData.Count; i++)
        {
            slammingForceData[i].previousVelocity = slammingForceData[i].velocity;
            Vector3 center = transform.TransformPoint(slammingForceData[i].triangleCenter);
            slammingForceData[i].velocity = BoatPhysicsMath.GetTriangleVelocity(boat, center);
        }
    }
}
