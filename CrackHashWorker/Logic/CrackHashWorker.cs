using System.Collections;
using System.Numerics;
using System.Security.Cryptography;
using System.Text;
using Combinatorics.Collections;
using DataContracts.Dto;

namespace Worker.Logic;

public class CrackHashWorker
{
    public async Task<List<string>> Run(CrackHashManagerRequestDto workerTaskInfo)
    {
        var words = new List<string>();
        
        var requiredHash = workerTaskInfo.Hash;
        var alphabet = workerTaskInfo.Alphabet;
        for (var currentWordLength = 0; currentWordLength <= workerTaskInfo.MaxLength; currentWordLength++)
        {
            var variations = new Variations<char>(alphabet!, currentWordLength, GenerateOption.WithRepetition);
            
            var lowBorder = variations.Count / workerTaskInfo.PartCount * (workerTaskInfo.PartNumber - 1);
            var highBorder = workerTaskInfo.PartNumber == workerTaskInfo.PartCount
                ? variations.Count
                : lowBorder + variations.Count / workerTaskInfo.PartCount;
            
            var enumerator = MoveForwardBy(variations.GetEnumerator(), lowBorder);
            
            Console.WriteLine($"HARD WORKING ({workerTaskInfo.RequestId}): I will hard work with {highBorder - lowBorder} tasks... ^-^");
            
            for (var currentIndex = lowBorder; currentIndex < highBorder; currentIndex++)
            {
                enumerator.MoveNext();
                List<char>? variation = (List<char>) enumerator.Current;
                var word = string.Join("", variation);
                var hash = MD5.HashData(Encoding.UTF8.GetBytes(word));
                if (requiredHash.Equals(Encoding.UTF8.GetString(hash)))
                {
                    words.Add(word);
                }
            }
        }

        return words;
    }

    private IEnumerator MoveForwardBy(IEnumerator enumerator, BigInteger position)
    {
        for (var i = 0; i < position; i++)
        {
            enumerator.MoveNext();
        }

        return enumerator;
    }
}