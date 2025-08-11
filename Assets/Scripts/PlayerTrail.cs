using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerTrail : MonoBehaviour
{
    float _meshRefreshRate = 0.04f;
        
    [SerializeField] GameObject cartaTrailPrefab;
    [SerializeField] Transform posToSpawn;
  
    List<IEnumerator> _cardTrailsCoroutines;
        
    int textureID = Shader.PropertyToID("_MainTex");

    void Awake()
    {
        _cardTrailsCoroutines = new List<IEnumerator>();
    }

    public void StartTrailVFX()
    {
        Coroutine coroutine = StartCoroutine(IEActivateCardTrails(0.7f));
    }
    
    IEnumerator IEActivateCardTrails(float timeActive)
    {
        while (timeActive > 0)
        {
            timeActive -= _meshRefreshRate;
            Vector2 pos = new Vector2(posToSpawn.position.x,posToSpawn.position.y);
            GameObject g = Instantiate(cartaTrailPrefab, pos, posToSpawn.rotation);
            g.transform.localScale = posToSpawn.localScale;
            Destroy(g,0.2f);
            yield return new WaitForSecondsRealtime(_meshRefreshRate);
        }
    }

    public void DestroyTrails()
    {
        StopAllCoroutines();
        enabled = false;
    }
}
