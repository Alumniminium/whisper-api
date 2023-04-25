# whisper-api

This API can only transcribe one file at a time. Every further request will wait for the current transcription to finish. This is because the [underlying library is a bit shit atm](https://github.com/sandrohanea/whisper.net/issues/28).

![](/demo.gif)

## Setup
```
git clone https://github.com/Alumniminium/whisper-api.git \
cd whisper-api \
docker build -t whisper-api:latest . \
docker run -p 80:80 whisper-api

# or

dotnet run
```
*models are stored in /models/, so you can mount a volume there. Please see [AI.cs](/AI.cs) for path and filenames*

*you can also set the environment variable `THREAD_COUNT` to change the number of threads used by the server. The default is half your CPU Cores, the threads are pinned to the first `n` cores without regard for hyperthreading or efficiency cores, see: `proc.ProcessorAffinity = (1 << THREAD_COUNT) - 1;` in `Program.cs`*

## Usage

### Get models
```
curl -X 'GET' 'https://localhost/models'
```
```
[
  "fast",
  "normal",
  "slow",
  "veryslow"
]
```
Please see [AI.cs](/AI.cs) for filename mappings*

### Speech to text
**stream lines as they become available**
```
curl -sNX 'POST' \
  'http://localhost/streamPlainText?model=veryslow&singleLine=false&includeTimestamps=true' \
  -F 'f=@The TV Screen.mp3'
```
**get full transcript**
```
curl -X 'POST' \
  'http://localhost/getPlainText?model=veryslow&singleLine=false&includeTimestamps=true' \
  -F 'f=@The TV Screen.mp3'
```
*[sample file](/The%20TV%20Screen.mp3) is included in the repo*
```
[00:00:00 - 00:00:20]	 [music]
[00:00:20 - 00:00:23]	 People look at their past just like it happened yesterday
[00:00:23 - 00:00:25]	 I try to see tomorrow like there ain't no other way
[00:00:25 - 00:00:28]	 People listen to the echo, I let go, I barely did it
[00:00:28 - 00:00:30]	 Rapping is something to prove so please move I need it finished
[00:00:30 - 00:00:33]	 But watch from the side because I need a witness
[00:00:33 - 00:00:35]	 So if I fall it is known that I did my business
[00:00:35 - 00:00:39]	 Man, and if I fall it wasn't all for nothing while you were out slacking off
[00:00:39 - 00:00:41]	 I was busy doing something so from here I write
[00:00:41 - 00:00:43]	 Day and night it's like I never get no sleep
[00:00:43 - 00:00:46]	 From here on out I'm about singing lyrics in my dreams
[00:00:46 - 00:00:49]	 I'll never quit, this is a dream surely fee now I believe I will achieve
[00:00:49 - 00:00:51]	 As long as I can take the heat
[00:00:51 - 00:00:53]	 I promised to myself that I will work on my production
[00:00:53 - 00:00:56]	 I promised to myself that I will begin to function
[00:00:56 - 00:00:59]	 I promised to myself that I will be someone so great
[00:00:59 - 00:01:01]	 Y'all will never forget me
[00:01:01 - 00:01:04]	 I will climb to the top eventually
[00:01:04 - 00:01:06]	 I will be someone that you can someday see
[00:01:06 - 00:01:09]	 I will never forget that this was on me
[00:01:09 - 00:01:11]	 Someday I will be on the TV screen
[00:01:11 - 00:01:14]	 I will climb to the top eventually
[00:01:14 - 00:01:16]	 I will be someone that you can someday see
[00:01:16 - 00:01:19]	 I will never forget that this was on me
[00:01:19 - 00:01:22]	 Someday I will be on the TV screen
[00:01:22 - 00:01:25]	 Yeah, I will make my way to the telly with a flat ballet
[00:01:25 - 00:01:29]	 And yet this is really a long long cold that I have got to come to grips with
[00:01:29 - 00:01:32]	 I've taken so long I should just quit the bitching
[00:01:32 - 00:01:34]	 There is no one else I blame but myself
[00:01:34 - 00:01:36]	 'Cause I was always here whenever I needed help
[00:01:36 - 00:01:39]	 Help, help, help me is all I seem to speak
[00:01:39 - 00:01:42]	 So I just called the quits when they turned the other cheek
[00:01:42 - 00:01:44]	 I've basically been a basic bitch my entire life
[00:01:44 - 00:01:47]	 I've got a rude awakening waiting for me without the light
[00:01:47 - 00:01:49]	 The light inside is right in front of me it's seen by my insight
[00:01:49 - 00:01:52]	 'Cause my insight puts open eyes lets me know that I'll be alright
[00:01:52 - 00:01:55]	 Alright, alright, alright so maybe I've begun to pick my feet up
[00:01:55 - 00:01:57]	 Hold up, hold up I need to speed up just like a cheetah
[00:01:57 - 00:02:00]	 Need my pair of hyperfuses time to burn up all my fuses
[00:02:00 - 00:02:02]	 The choice I choose must never be a naked muses
[00:02:02 - 00:02:05]	 I will climb to the top eventually
[00:02:05 - 00:02:08]	 I will be someone that you can someday see
[00:02:08 - 00:02:10]	 I will never forget that this was on me
[00:02:10 - 00:02:13]	 Someday I will be on the TV screen
[00:02:13 - 00:02:15]	 I will climb to the top eventually
[00:02:15 - 00:02:18]	 I will be someone that you can someday see
[00:02:18 - 00:02:20]	 I will never forget that this was on me
[00:02:20 - 00:02:23]	 Someday I will be on the TV screen
[00:02:23 - 00:02:25]	 Someday I'll contribute to society
[00:02:25 - 00:02:28]	 And someday we'll get past the advisory
[00:02:28 - 00:02:29]	 My mind is like a bunch of written binary
[00:02:29 - 00:02:33]	 My mind has no privacy finally I've begun to begin the rioting
[00:02:33 - 00:02:35]	 Ironically the darkness seems to follow me
[00:02:35 - 00:02:38]	 But this time around I'm making a break I promise thee
[00:02:38 - 00:02:40]	 I'm on my grind like a big beard blacksmith
[00:02:40 - 00:02:43]	 Against paper my brain is something I'll now attack with
[00:02:43 - 00:02:45]	 I swear to myself that I'll hunt my way to fame
[00:02:45 - 00:02:48]	 I swear on everything even my j-hawk name
[00:02:48 - 00:02:51]	 This is for honor, guts, glory and betting round the clock
[00:02:51 - 00:02:53]	 The day that I quit rapping is the day my breath stops
[00:02:53 - 00:02:56]	 So if I fall to the mat I hope you never count me out
[00:02:56 - 00:02:59]	 I will see it to the end that I finish this bout
[00:02:59 - 00:03:01]	 You can go ahead and make your way to ten
[00:03:01 - 00:03:03]	 But you better let 'em know that I'll rise before then
[00:03:03 - 00:03:06]	 I will climb to the top eventually
[00:03:06 - 00:03:09]	 I will be someone that you can someday see
[00:03:09 - 00:03:11]	 I will never forget that this was on me
[00:03:11 - 00:03:14]	 Someday I will be on the TV screen
[00:03:14 - 00:03:16]	 I will climb to the top eventually
[00:03:16 - 00:03:19]	 I will be someone that you can someday see
[00:03:19 - 00:03:21]	 I will never forget that this was on me
[00:03:21 - 00:03:24]	 Someday I will be on the TV screen
[00:03:24 - 00:03:43]	 [Music]
```

## Sample Script
> transcribe.sh
```
#!/bin/bash

if [[ $# -eq 0 ]]; then
  echo "Usage: $0 <filename>"
  exit 1
fi

filename="$1"
model="veryslow"
singleLine="false"
timestamps="true"
output_file="${filename%.*}.txt"

curl -sNX 'POST' "https://localhost/streamPlainText?model=$model&singleLine=$singleLine&includeTimestamps=$timestamps" -F "f=@$filename" | tee "$output_file"
```