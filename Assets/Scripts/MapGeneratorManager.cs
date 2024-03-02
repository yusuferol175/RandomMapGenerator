using System.Collections;
using System.Collections.Generic;
using System.Linq;
using DG.Tweening;
using UnityEngine;
using UnityEngine.Serialization;
using UnityEngine.UI;

public class MapGeneratorManager : MonoBehaviour
{
    [Header("UI Props")] 
    [SerializeField] private Button _generateMapButton;

    [Header("Path Props")] 
    [SerializeField] private GameObject _pathPrefab;
    [SerializeField] private GameObject _pathsGroup;

    [System.Serializable]
    private class Path
    {
        [SerializeField] private Sprite _pathSprite;
        [SerializeField] private Vector4 _pathEdge;

        public Sprite PathSprite => _pathSprite;
        public Vector4 PathEdge => _pathEdge;
    }

    [SerializeField] private List<Path> Paths;

    [Header("Matrix Count")] 
    [SerializeField] private int _matrixCount;

    private Path _path;
    private List<Path> _randomPathList = new();
    private List<Path> _referencesPathList = new();

    void Start()
    {
        _generateMapButton.onClick.AddListener(() => StartCoroutine(GenerateMap()));
    }

    private IEnumerator GenerateMap()
    {
        _generateMapButton.interactable = false;

        ResetMap();

        float pathCount = Mathf.Pow(_matrixCount, 2);

        Vector3 pathStartPos = _pathsGroup.transform.position;
        Vector3 createPos = pathStartPos;

        for (int i = 1; i < pathCount + 1; i++)
        {
            GameObject newPath = Instantiate(_pathPrefab, createPos, Quaternion.identity, _pathsGroup.transform);

            createPos = new Vector3(createPos.x + 2f, createPos.y, createPos.z);

            newPath.transform.DOScale(1, .5f);

            if (i != 1)
            {
                if (_referencesPathList.Count < _matrixCount)
                {
                    foreach (var path in Paths)
                    {
                        if (path.PathEdge.x == _path.PathEdge.z)
                            _randomPathList.Add(path);
                    }

                    GetNextPath();
                }
                else
                {
                    int referenceIndex = i - _matrixCount - 1;

                    Path referencesPath = _referencesPathList[referenceIndex];

                    if (referenceIndex == 0)
                    {
                        foreach (var path in Paths)
                        {
                            if (path.PathEdge.y == referencesPath.PathEdge.w)
                                _randomPathList.Add(path);
                        }

                        GetNextPath();
                    }
                    else
                    {
                        foreach (var path in Paths)
                        {
                            if (path.PathEdge.y == referencesPath.PathEdge.w && path.PathEdge.x == _path.PathEdge.z)
                                _randomPathList.Add(path);
                        }

                        GetNextPath();
                    }
                }
            }
            else
            {
                _path = Paths[Random.Range(0, Paths.Count)];
                _referencesPathList.Add(_path);
            }

            newPath.GetComponent<SpriteRenderer>().sprite = _path.PathSprite;

            if (i % _matrixCount == 0)
                createPos = new Vector3(pathStartPos.x, createPos.y - 2f, pathStartPos.z);

            yield return new WaitForSeconds(.5f);
        }

        _generateMapButton.interactable = true;
    }

    private void GetNextPath()
    {
        _path = _randomPathList[Random.Range(0, _randomPathList.Count)];
        _referencesPathList.Add(_path);

        _randomPathList.Clear();
    }

    private void ResetMap()
    {
        for (int i = 0; i < _pathsGroup.transform.childCount; i++)
            Destroy(_pathsGroup.transform.GetChild(i).gameObject);

        _path = null;
        _referencesPathList.Clear();
    }
}