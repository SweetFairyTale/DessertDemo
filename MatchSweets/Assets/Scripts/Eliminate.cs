using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Eliminate : MonoBehaviour {

    public AnimationClip ClearAnim;

    private bool eliminating;    

    public bool Eliminating
    {
        get
        {
            return eliminating;
        }
    }

    protected SweetsController sweet;

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
            yield return new WaitForSeconds(ClearAnim.length);
            Destroy(gameObject);
        }
    }
}
