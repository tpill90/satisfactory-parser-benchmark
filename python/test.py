
import sav_parse
import gc
import time
from typing import List
import statistics
from rich import box
from rich.console import Console
from rich.table import Table
from rich.rule import Rule
from rich.text import Text
from rich.align import Align

iterations = 10
console = Console()

def print_results_summary(run_elapsed_times: List[int]):
	# Calculating stats
	fastest_time = min(run_elapsed_times)
	slowest_time = max(run_elapsed_times)
	average = statistics.mean(run_elapsed_times)

	# Building out summary table
	summary_table = Table(box=box.MINIMAL_HEAVY_HEAD, show_header=False)
	summary_table.add_column("Min", justify="center", style="cyan")
	summary_table.add_column("Average", justify="center", style="yellow")
	summary_table.add_column("Max", justify="center", style="medium_purple4")
	summary_table.add_row(f"{fastest_time}ms", f"{round(average)}ms", f"{slowest_time}ms")

	# Setting up final formatting, to make sure padding and alignment is correct
	console.print(Rule())
	console.print(Align(Text("Benchmark summary", style="bold white"), align="left"))
	console.print(summary_table)

run_results: List[float] = []
for i in range(iterations):

	# Force GC between runs, to minimize leftover objects from the previous run.
	gc.collect()

	# Run the save parser
	start_time = time.perf_counter()
	sav_parse.readFullSaveFile("../SampleSave.sav")

	# Save and log the results
	end_time = time.perf_counter()
	elapsed_ms = round((end_time - start_time) * 1000)
	run_results.append(elapsed_ms)

	console.print(f"Run [cyan]{i + 1}[/cyan] finished in [yellow]{elapsed_ms}ms[/yellow]")

print_results_summary(run_results)

