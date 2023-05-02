import os
import Levenshtein
import jiwer
import re
import string
import sys

def number_to_string(match):
    number_dict = {
        '0': 'zero',
        '1': 'one',
        '2': 'two',
        '3': 'three',
        '4': 'four',
        '5': 'five',
        '6': 'six',
        '7': 'seven',
        '8': 'eight',
        '9': 'nine'
    }
    return number_dict[match.group()]

automated_transcripts = []
automated_filenames = []
for filename in os.listdir('.'):
    if filename.startswith('whisper'):
        with open(filename) as f:
            text = f.read()
            text = re.sub(r'^\s*\[.*?\]\s*', '', text)
            text = re.sub(r'\s*\[.*?\]\s*$', '', text)
            text = re.sub(r'\s*\{END\}.*$', '', text)
            text = re.sub(r'\s*\[.*?\]$', '', text)
            text = text.translate(str.maketrans('', '', string.punctuation))
            text = re.sub(r'\d', number_to_string, text)
            automated_transcripts.append(text.lower())
            automated_filenames.append(filename)

manual_transcript = open(sys.argv[1]).read().lower()
manual_transcript = manual_transcript.translate(str.maketrans('', '', string.punctuation))
manual_transcript = re.sub(r'^\s*\[.*?\]\s*', '', manual_transcript)
manual_transcript = re.sub(r'\s*\[.*?\]\s*$', '', manual_transcript)
manual_transcript = re.sub(r'\s*\{END\}.*$', '', manual_transcript)
manual_transcript = re.sub(r'\s*\[.*?\]$', '', manual_transcript)
manual_transcript = re.sub(r'\d', number_to_string, manual_transcript)
manual_transcript = manual_transcript.lower()

manual_words = manual_transcript.strip().lower().split()
automated_words = [t.strip().lower().split() for t in automated_transcripts]

lev_scores = [Levenshtein.distance(manual_words, words) for words in automated_words]
wer_scores = [jiwer.wer(manual_transcript, t) for t in automated_transcripts]
final_scores = [0.5 * lev + 0.5 * wer for lev, wer in zip(lev_scores, wer_scores)]

best_transcript_index = final_scores.index(min(final_scores))
best_transcript = automated_transcripts[best_transcript_index]
best_filename = automated_filenames[best_transcript_index]

# Sort the transcripts by score
sorted_scores = sorted(zip(final_scores, wer_scores, lev_scores, automated_filenames, automated_transcripts))

# Print out the scores for each model
for score, wer, lev, filename, transcript in sorted_scores:
    print(f"Score: {score:.2f} WER: {wer:.2f} Levenshtein: {lev:.2f} - {filename}")
print('\nThe filename of the best transcript is:', best_filename)
