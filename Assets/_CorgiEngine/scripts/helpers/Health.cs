using UnityEngine;
using System.Collections;
/// <summary>
/// Adds this class to an object so it can have health (and lose it)
/// </summary>
public class Health : MonoBehaviour, CanTakeDamage
{
	/// the current amount of health of the object
	public int CurrentHealth;
	/// the points the player gets when the object's health reaches zero
	public int PointsWhenDestroyed;
	/// the effect to instantiate when the object takes damage
	public GameObject HurtEffect;
	/// the effect to instantiate when the object gets destroyed
	public GameObject DestroyEffect;

	/// <summary>
	/// What happens when the object takes damage
	/// </summary>
	/// <param name="damage">Damage.</param>
	/// <param name="instigator">Instigator.</param>
	public void TakeDamage(int damage,GameObject instigator)
	{	
		// when the object takes damage, we instantiate its hurt effect
		Instantiate(HurtEffect,instigator.transform.position,transform.rotation);
		// and remove the specified amount of health	
		CurrentHealth -= damage;
		// if the object doesn't have health anymore, we destroy it
		if (CurrentHealth<=0)
		{
			DestroyObject();
            return;
		}

        // We make the character's sprite flicker
        Color initialColor = GetComponent<Renderer>().material.color;
        Color flickerColor = new Color32(255, 20, 20, 255);
        StartCoroutine(Flicker(initialColor, flickerColor, 0.02f));	
	}

    /// <summary>
    /// Coroutine used to make the character's sprite flicker (when hurt for example).
    /// </summary>
    IEnumerator Flicker(Color initialColor, Color flickerColor, float flickerSpeed)
    {
        for (var n = 0; n < 10; n++)
        {
            GetComponent<Renderer>().material.color = initialColor;
            yield return new WaitForSeconds(flickerSpeed);
            GetComponent<Renderer>().material.color = flickerColor;
            yield return new WaitForSeconds(flickerSpeed);
        }
        GetComponent<Renderer>().material.color = initialColor;

        // makes the character colliding again with layer 12 (Projectiles) and 13 (Enemies)
        Physics2D.IgnoreLayerCollision(9, 12, false);
        Physics2D.IgnoreLayerCollision(9, 13, false);
    }
	

	/// <summary>
	/// Destroys the object
	/// </summary>
	private void DestroyObject()
	{
		// instantiates the destroy effect
		if (DestroyEffect!=null)
		{
			var instantiatedEffect=(GameObject)Instantiate(DestroyEffect,transform.position,transform.rotation);
            instantiatedEffect.transform.localScale = transform.localScale;
		}
		// Adds points if needed.
		if(PointsWhenDestroyed != 0)
		{
			GameManager.Instance.AddPoints(PointsWhenDestroyed);
		}
		// destroys the object
		gameObject.SetActive(false);
	}
}
