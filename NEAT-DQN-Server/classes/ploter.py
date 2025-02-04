import matplotlib.pyplot as plt
import os
import csv
import pandas as pd

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
line1_1, = ax1.plot([], [], label='Najwięcej ruchów w generacji')
line1_2, = ax1.plot([], [], label='Najmniej ruchów w generacji')
line1_3, = ax1.plot([], [], label='Średnio ruchów w generacji')

ax1.set_xlabel('Generacja')
ax1.set_ylabel('Ilość')
ax1.legend()
ax1.grid(True)

fig6, ax6 = plt.subplots(figsize=(10, 6))
line6_1, = ax6.plot([], [], label='Wyuczone ruchy')
line6_2, = ax6.plot([], [], label='Losowe ruchy')

ax6.set_xlabel('Generacja')
ax6.set_ylabel('Ilość')
ax6.set_title('Rodzaj ruchu wykoanany przez agenta DQN')
ax6.legend()
ax6.grid(True)

fig5, ax5 = plt.subplots(figsize=(10, 6))
line5_1, = ax5.plot([], [], label='Głód')
line5_2, = ax5.plot([], [], label='Drapieżnik')

ax5.set_xlabel('Generacja')
ax5.set_ylabel('Ilość')
ax5.set_title('Powód śmierci agenta')
ax5.legend()
ax5.grid(True)

fig10, ax10 = plt.subplots(figsize=(10, 6))

def timePloter(best, worst, avg, filePath, model):
    ax1.set_title(f'Liczba ruchów agentów {model} w symulacji')

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
            writer.writerow(["Generacja", "Najwyższa ocena w generacji", "Najniższa ocena w generacji", "Średnia ocena generacji"])
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
def movesPloter(calculated, random, filePath, model):
    generations = list(range(1, len(calculated) + 1))

    line6_1.set_data(generations, calculated)
    line6_2.set_data(generations, random)

    ax6.set_xlim(0, len(generations) + 1)
    ax6.set_ylim(min(min(calculated), min(random)) - 1, max(max(calculated), max(random)) + 1)

    fig6.canvas.draw()
    fig6.canvas.flush_events()

    if filePath != "":
        pathToSave = os.path.join(filePath, f'{model}_MoveType_Generation{len(generations)}.png')
        fig6.savefig(pathToSave)

        pathToSave = os.path.join(filePath, f'{model}_MoveType_Generation{len(generations)}.csv')
        with open(pathToSave, mode='w', newline='') as file:
            writer = csv.writer(file)
            writer.writerow(["Generacja", "Wyuczone ruchy", "Losowe ruchy"])
            for gen, max_f, min_f in zip(generations, calculated, random):
                writer.writerow([gen, max_f, min_f])
def deathPloter(food, enemy, filePath, model):
    generations = list(range(1, len(food) + 1))

    line5_1.set_data(generations, food)
    line5_2.set_data(generations, enemy)

    ax5.set_xlim(0, len(generations)+1)
    ax5.set_ylim(min(min(food), min(enemy)) - 1, max(max(food), max(enemy)) + 1)

    fig5.canvas.draw()
    fig5.canvas.flush_events()

    if filePath != "":
        pathToSave = os.path.join(filePath, f'{model}_CauseOfDeath_Generation{len(generations)}.png')
        fig5.savefig(pathToSave)

        pathToSave = os.path.join(filePath, f'{model}_CauseOfDeath_Generation{len(generations)}.csv')
        with open(pathToSave, mode='w', newline='') as file:
            writer = csv.writer(file)
            writer.writerow(["Generacja", "Głód", "Przeciwnik"])
            for gen, max_f, min_f in zip(generations, food, enemy):
                writer.writerow([gen, max_f, min_f])
def decisionPoter(decisionsData, filePath, model, generations):

    summed_data = {}
    for key, subdict in decisionsData.items():
        for direction, types in subdict.items():
            if direction not in summed_data:
                summed_data[direction] = {}
            for type_, values in types.items():
                if type_ not in summed_data[direction]:
                    summed_data[direction][type_] = [0] * len(values)
                summed_data[direction][type_] = [sum(x) for x in zip(summed_data[direction][type_], values)]

    percent_data = {}
    for direction in summed_data:
        percent_data[direction] = {}
        for type_, values in summed_data[direction].items():
            row_sum = sum(values)
            percent_data[direction][type_] = [v / row_sum * 100 if row_sum != 0 else 0 for v in values]

    rows = []
    columns = ["W górę", "W prawo", "W dół", "W lewo"]

    header = [""] + columns

    for direction in percent_data:
        for type_ in percent_data[direction]:
            row = [f"{direction} {type_}"] + [f"{value:.2f}%" for value in percent_data[direction][type_]]
            rows.append(row)

    table = ax10.table(cellText=[header] + rows, colLabels=None, cellLoc='center', loc='center', bbox=[0, 0, 1, 1])

    ax10.set_title(f"Procentowe decyzje Agenta {model}", fontsize=16)

    ax10.axis('off')

    fig10.canvas.draw()
    fig10.canvas.flush_events()

    if filePath != "":
        pathToSave = os.path.join(filePath, f'{model}_Decisions_Generation{generations}.png')
        fig10.savefig(pathToSave)

        pathToSave = os.path.join(filePath, f'{model}_Decisions_Generation{generations}.csv')
        with open(pathToSave, mode='w', newline='') as file:
            writer = csv.writer(file)
            writer.writerow(header)
            for row in rows:
                writer.writerow(row)
