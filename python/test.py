
import sav_parse
import time

for x in range(6):
    start_time = time.time()  # Record the start time
    sav_parse.readFullSaveFile("C:\\Users\\Tim\\Dropbox\\Programming\\dotnet\\SatisfactorySaveParserBenchmark\\SampleSave-Small.sav")
    end_time = time.time()  # Record the end time

    elapsed_time = (end_time - start_time) * 1000  # Calculate elapsed time in milliseconds
    print(f"{elapsed_time:.2f} ms to execute.")
