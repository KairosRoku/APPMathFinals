using UnityEngine;
using System.Collections;
using UnityEngine.UI;

public class Coin : MonoBehaviour
{
    private Transform targetUI; // We need to find the UI coin icon
    private float speed = 15f;
    private bool performFly = false;
    private Vector3 targetPos;

    private void Start()
    {
        // Find CoinUI in scene (Tag it or Singleton)
        GameObject uiObj = GameObject.FindGameObjectWithTag("CoinUI");
        if (uiObj != null)
        {
            targetUI = uiObj.transform;
            targetPos = Camera.main.ScreenToWorldPoint(targetUI.position);
            targetPos.z = 0; // Ensure 2D
            
            StartCoroutine(FlyDelay());
        }
        else
        {
            // Just destroy if no UI
             Destroy(gameObject, 0.5f);
        }
    }

    IEnumerator FlyDelay()
    {
        // Little "pop" effect before flying
        Vector3 startScale = transform.localScale;
        Vector3 endScale = startScale * 1.5f;
        float t = 0;
        
        while(t < 0.2f)
        {
            t += Time.deltaTime;
            transform.localScale = Vector3.Lerp(startScale, endScale, t/0.2f);
            yield return null;
        }

        yield return new WaitForSeconds(0.3f);
        performFly = true;
    }

    private int _value = 10;

    public void SetValue(int val)
    {
        _value = val;
    }

    private void Update()
    {
        if (!performFly || targetUI == null) return;
        
        Vector3 screenPoint = targetUI.position;
        screenPoint.z = 10.0f; 
        Vector3 worldPoint = Camera.main.ScreenToWorldPoint(screenPoint);
        worldPoint.z = 0;

        transform.position = Vector3.MoveTowards(transform.position, worldPoint, speed * Time.deltaTime);

        if (Vector3.Distance(transform.position, worldPoint) < 0.5f)
        {
            GameManager.Instance.AddGold(_value);
             Destroy(gameObject);
        }
    }
}
