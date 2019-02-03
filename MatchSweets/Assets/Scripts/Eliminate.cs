using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Eliminate : MonoBehaviour {

    public AnimationClip ClearAnim;

    private bool eliminating;

    public AudioClip destroy;

    protected SweetsController sweet;

    private void Awake()
    {
        sweet = GetComponent<SweetsController>();    
    }

    public bool Eliminating
    {
        get
        {
            return eliminating;
        }
    }

    

    public virtual void Elimi()
    {
        eliminating = true;
        StartCoroutine(ClearCor());
    }

    private IEnumerator ClearCor()
    {
        Animator anim = GetComponent<Animator>();
        if(anim!=null)
        {
            anim.Play(ClearAnim.name);
            GameManager.Instance.playerScore++;
            AudioSource.PlayClipAtPoint(destroy, transform.position);
            yield return new WaitForSeconds(ClearAnim.length);
            Destroy(gameObject);
        }
    }
}
