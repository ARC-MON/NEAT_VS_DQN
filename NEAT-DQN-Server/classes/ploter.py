import matplotlib.pyplot as plt
import os
import csv

plt.ion()

fig2, ax2 = plt.subplots(figsize=(10, 6))
line2_1, = ax2.plot([], [], label='Najwyższa ocena w generacji')
line2_2, = ax2.plot([], [], label='Najniższa ocena w generacji')
line2_3, = ax2.plot([], [], label='Średnia ocena generacji')

ax2.set_xlabel('Generacja')
ax2.set_ylabel('Ocena')

ax2.legend()
ax2.grid(True)

fig8, ax8 = plt.subplots(figsize=(10, 6))
line8_1, = ax8.plot([], [], label='Procesor - zużycie')
line8_2, = ax8.plot([], [], label='Procesor - zużycie średnie')
line8_3, = ax8.plot([], [], label='Pamięć - zużycie')
line8_4, = ax8.plot([], [], label='Pamięć - zużycie średnie')

ax8.set_xlabel('Generacja')
ax8.set_ylabel('Procent')
ax8.legend()
ax8.grid(True)

fig1, ax1 = plt.subplots(figsize=(10, 6))
line1_1, = ax1.plot([], [], label='Najdłuższy czas w generacji')
line1_2, = ax1.plot([], [], label='Najkrótszy czas w generacji')
line1_3, = ax1.plot([], [], label='Średni czas w generacji')

ax1.set_xlabel('Generacja')
ax1.set_ylabel('Czas')
ax1.legend()
ax1.grid(True)
def timePloter(best, worst, avg, filePath, model):
    ax1.set_title(f'Czas agentów {model} w symulacji')

    generations = list(range(1, len(best) + 1))

    line1_1.set_data(generations, best)
    line1_2.set_data(generations, worst)
    line1_3.set_data(generations, avg)

    ax1.set_xlim(0, len(generations)+1)
    ax1.set_ylim(min(min(best), min(worst), min(avg)) - 1, max(max(best), max(worst), max(avg)) + 1)

    fig1.canvas.draw()
    fig1.canvas.flush_events()

    if filePath != "":
        pathToSave = os.path.join(filePath, f'{model}_agents_times_Generation{len(generations)}.png')
        fig1.savefig(pathToSave)

        pathToSave = os.path.join(filePath, f'{model}_agents_times_Generation{len(generations)}.csv')
        with open(pathToSave, mode='w', newline='') as file:
            writer = csv.writer(file)
            writer.writerow(["Generacja", "Najdłuższy czas w generacji", "Najkrótszy czas w generacji", "Średni czas w generacji"])
            for gen, max_f, min_f, avg_f in zip(generations, best, worst, avg):
                writer.writerow([gen, max_f, min_f, avg_f])
def fitPloter(best, worst, avg, filePath, model):
    ax2.set_title(f'Oceny agentów {model}')

    generations = list(range(1, len(best) + 1))

    line2_1.set_data(generations, best)
    line2_2.set_data(generations, worst)
    line2_3.set_data(generations, avg)

    ax2.set_xlim(0, len(generations) + 1)
    ax2.set_ylim(min(min(best), min(worst), min(avg)) - 1, max(max(best), max(worst), max(avg)) + 1)

    fig2.canvas.draw()
    fig2.canvas.flush_events()

    if filePath != "":
        pathToSave = os.path.join(filePath, f'{model}_Fitness_Generation{len(generations)}.png')
        fig2.savefig(pathToSave)

        pathToSave = os.path.join(filePath, f'{model}_Fitness_Generation{len(generations)}.csv')
        with open(pathToSave, mode='w', newline='') as file:
            writer = csv.writer(file)
            writer.writerow(["Generacja", "Najwyższa ocena w generacji", "Najniższa ocena w generacji", "Średnia ocena generacji"])
            for gen, max_f, min_f, avg_f in zip(generations, best, worst, avg):
                writer.writerow([gen, max_f, min_f, avg_f])
def cpuPloter(cpu, avgcpu, memory, avgmemory, filePath, model):
    ax8.set_title(f'Zużycie procesora i pamięci przez algorytm {model}')

    generations = list(range(1, len(cpu) + 1))

    line8_1.set_data(generations, cpu)
    line8_2.set_data(generations, avgcpu)
    line8_3.set_data(generations, memory)
    line8_4.set_data(generations, avgmemory)

    ax8.set_xlim(0, len(generations) + 10)
    ax8.set_ylim(min(min(cpu), min(memory)) - 10, max(max(cpu), max(memory)) + 10)

    fig8.canvas.draw()
    fig8.canvas.flush_events()

    if filePath != "":
        pathToSave = os.path.join(filePath, f'{model}_CPU_Memory_Generation{len(generations)}.png')
        fig8.savefig(pathToSave)

        pathToSave = os.path.join(filePath, f'{model}_CPU_Memory_Generation{len(generations)}.csv')
        with open(pathToSave, mode='w', newline='') as file:
            writer = csv.writer(file)
            writer.writerow(["Generacja", "Procesor - zużycie", "Procesor - zużycie średnie", "Pamięć - zużycie", "Pamięć - zużycie średnie"])
            for gen, max_f, min_f, avg_f, avg_m in zip(generations, cpu, avgcpu, memory, avgmemory):
                writer.writerow([gen, max_f, min_f, avg_f])