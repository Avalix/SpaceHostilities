﻿using UnityEngine;

[ExecuteInEditMode]
public class FactionSpawnPoint : MonoBehaviour {
	
	public ShipFactionDataSO Faction;
	public float SpawnRadius = 75.0f;
	
	private Renderer _renderer;
	private MaterialPropertyBlock _propBlock;
	
	private void Update()
	{
#if UNITY_EDITOR
		if(_renderer == null)
		{
			_renderer = GetComponent<Renderer>();
		}
		
		if(_propBlock == null)
		{
			_propBlock = new MaterialPropertyBlock();
		}
		
		Color sphereColor = Color.gray;
		float emmision = 0.0f;
		
		if(Faction!= null)
		{
			sphereColor = Faction.Data.FactionColor * 2.0f;
			emmision = 0.0f;
		}
		
		_renderer.GetPropertyBlock(_propBlock);
		_propBlock.SetColor("_Color", sphereColor);
		_propBlock.SetFloat("_Emmision", emmision);
		_renderer.SetPropertyBlock(_propBlock);
#endif
	}


	private void OnDrawGizmosSelected()
	{
		Gizmos.color = Color.white;
		
		if(Faction != null)
		{
			Gizmos.color = Faction.Data.FactionColor;
		}
		
		Gizmos.DrawWireSphere(transform.position, SpawnRadius);
	}
}
