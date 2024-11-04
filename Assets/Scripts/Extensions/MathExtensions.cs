using Photon.Pun;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public static class MathExtensions
{
    public static Vector3 GetReverseDirection()
    {
        return GetPlayerDirection() == Vector3.right ? Vector3.left : Vector3.right;
    }

    public static Vector3 GetPlayerDirection()
    {
        var currentPlayer = TrackManager.instance.characterController.character;
        var nearestCharacter = GetNearestPlayer();

        float playerDistance = currentPlayer.transform.localPosition.x - nearestCharacter.transform.localPosition.x;

        return playerDistance > 0 ? Vector3.left : Vector3.right;
    }

    public static Character GetNearestPlayer()
    {
        var currentPlayer = TrackManager.instance.characterController.character;

        var otherPlayers = Object.FindObjectsOfType<Character>().ToList();
        otherPlayers.Remove(currentPlayer);

        float tempDistance = 1000000;
        Character player = null;

        foreach (var otherPlayer in otherPlayers)
        {
            float distance = Vector3.Distance(currentPlayer.transform.position, otherPlayer.transform.position);

            if (distance < tempDistance)
            {
                player = otherPlayer;
                tempDistance = distance;
            }
        }

        return player;
    }

    public static List<Character> GetNearestPlayersByDirection(Character character, Vector3 direction, float distance, int maxPlayer)
    {
        Dictionary<Character, float> characters = new Dictionary<Character, float>();

        var otherPlayers = Object.FindObjectsOfType<Character>().ToList();
        otherPlayers.Remove(character);

        foreach (var otherPlayer in otherPlayers)
        {
            float playerDistance = character.transform.localPosition.x - otherPlayer.transform.localPosition.x;

            if (Mathf.Abs(playerDistance) > distance)
                continue;

            Vector3 playerDirection = playerDistance > 0 ? Vector3.left : Vector3.right;

            if (direction == playerDirection && characters.Count < maxPlayer && characters.ContainsKey(otherPlayer) == false)
            {
                characters.Add(otherPlayer, Mathf.Abs(playerDistance));
            }
        }

        var sorteredPlayers = characters.OrderBy(character => character.Value).Select(character => character.Key).ToList();

        return sorteredPlayers;
    }
}
