"""Computes diffs of lines."""

from dataclasses import dataclass
#import numpy as np


@dataclass(frozen=True)
class Addition:
    """Represents an addition in a diff."""
    content: str


@dataclass(frozen=True)
class Removal:
    """Represents a removal in a diff."""
    content: str


@dataclass(frozen=True)
class Unchanged:
    """Represents something unchanged in a diff."""
    content: str


def _compute_longest_common_subsequence(text1, text2):
    """Computes the longest common subsequence of the two given strings.

    The result is a table where cell (i, j) tells you the length of the
    longest common subsequence of text1[:i] and text2[:j].
    """
    n = len(text1)
    m = len(text2)

    lcs = [[None for _ in range(m + 1)]
           for _ in range(n + 1)]

    for i in range(0, n + 1):
        for j in range(0, m + 1):
            if i == 0 or j == 0:
                lcs[i][j] = 0
            elif text1[i - 1] == text2[j - 1]:
                lcs[i][j] = 1 + lcs[i - 1][j - 1]
            else:
                lcs[i][j] = max(lcs[i - 1][j], lcs[i][j - 1])

    return lcs

# Space optimized function to find the length of the longest common subsequence
# of substring `X[0…m-1]` and `Y[0…n-1]`


def LCSLength(X, Y, m, n):

    # Allocate storage for one-dimensional list `curr`

    # This occupies more space as None takes 16 bytes
    curr = [None] * (n + 1)

    # This occupies very less spacea as unit8 takes 1 byte
    #curr = np.empty(n+1, np.uint8)

    # Fill the lookup table in a bottom-up manner

    for i in range(m + 1):
        prev = curr[0]
        for j in range(n + 1):
            backup = curr[j]
            if i == 0 or j == 0:
                curr[j] = 0
            else:
                # if the current character of `X` and `Y` matches
                if X[i - 1] == Y[j - 1]:
                    curr[j] = prev + 1
                # otherwise, if the current character of `X` and `Y` don't match
                else:
                    curr[j] = max(curr[j], curr[j - 1])

            prev = backup

    # LCS will be the last entry in the lookup table
    return curr[n]


def diff(text1, text2):
    """Computes the optimal diff of the two given inputs.

    The result is a list where all elements are Removals, Additions or
    Unchanged elements.
    """
    # Since, Making Matrix takes very large memory, we will get Length of Longest Common Subsequence separetly for each cell of Matrix
    # Note: While using the below function, we don't need to use LCSLength indvidually, since we will already have Matrix containing these lengths
    # and we can these length by index of Matrix.
    # lcs = _compute_longest_common_subsequence(text1, text2)

    results = []

    i = len(text1)
    j = len(text2)

    while i != 0 or j != 0:
        # If we reached the end of text1 (i == 0) or text2 (j == 0), then we
        # just need to print the remaining additions and removals.
        if i == 0:
            results.append(Addition(text2[j - 1]))
            j -= 1
        elif j == 0:
            results.append(Removal(text1[i - 1]))
            i -= 1
        # Otherwise there's still parts of text1 and text2 left. If the
        # currently considered part is equal, then we found an unchanged part,
        # which belongs to the longest common subsequence.
        elif text1[i - 1] == text2[j - 1]:
            results.append(Unchanged(text1[i - 1]))
            i -= 1
            j -= 1
        # In any other case, we go in the direction of the longest common
        # subsequence.
        elif LCSLength(text1, text2, i-1, j) <= LCSLength(text1, text2, i, j-1):
            results.append(Addition(text2[j - 1]))
            j -= 1
        else:
            results.append(Removal(text1[i - 1]))
            i -= 1

    return list(reversed(results))


def format(diff_unified):
    """Format Unified View "difference" to thier respective @dataclass: Addition, Removal, and Unchanged

    The result is a list where all elements are Removals, Additions or
    Unchanged elements.
    """
    results = []

    for difference in diff_unified:
        if(difference[0] == '+'):
            results.append(Addition(difference[1:]))
        elif(difference[0] == '-'):
            results.append(Removal(difference[1:]))
        elif(difference[0] == ' '):
            results.append(Unchanged(difference[1:]))

    return results
