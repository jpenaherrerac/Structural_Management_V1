\# BANCO DE DATOS NORMATIVO – E.030 (MARKDOWN COMPLETO)

Todas las tablas normativas extraídas hasta el momento

\## TABLA N° 3 – FACTOR DE SUELO “S”

{

&nbsp; "Tabla\_003\_FactorSueloS": \[

&nbsp;   {"Zona": "Z4", "S0": 0.80, "S1": 1.00, "S2": 1.05, "S3": 1.10},

&nbsp;   {"Zona": "Z3", "S0": 0.80, "S1": 1.00, "S2": 1.15, "S3": 1.20},

&nbsp;   {"Zona": "Z2", "S0": 0.80, "S1": 1.00, "S2": 1.20, "S3": 1.40},

&nbsp;   {"Zona": "Z1", "S0": 0.80, "S1": 1.00, "S2": 1.60, "S3": 2.00}

&nbsp; ]

}





TABLA N° 4 – PERÍODOS TP Y TL

{

&nbsp; "Tabla\_004\_TP\_TL": \[

&nbsp;   {"Perfil": "S0", "TP": 0.30, "TL": 3.00},

&nbsp;   {"Perfil": "S1", "TP": 0.40, "TL": 2.50},

&nbsp;   {"Perfil": "S2", "TP": 0.60, "TL": 2.00},

&nbsp;   {"Perfil": "S3", "TP": 1.00, "TL": 1.60}

&nbsp; ]

}





TABLA N° 7 – SISTEMAS ESTRUCTURALES (R₀)

{

&nbsp; "Tabla\_007\_SistemasEstructurales": \[

&nbsp;   {"Sistema": "SMF", "Descripcion": "Pórticos Especiales Resistentes a Momentos", "R0": 8},

&nbsp;   {"Sistema": "IMF", "Descripcion": "Pórticos Intermedios Resistentes a Momentos", "R0": 5},

&nbsp;   {"Sistema": "OMF", "Descripcion": "Pórticos Ordinarios Resistentes a Momentos", "R0": 4},

&nbsp;   {"Sistema": "SCBF", "Descripcion": "Pórticos Especiales Concéntricamente Arriostrados", "R0": 7},

&nbsp;   {"Sistema": "OCBF", "Descripcion": "Pórticos Ordinarios Concéntricamente Arriostrados", "R0": 4},

&nbsp;   {"Sistema": "EBF", "Descripcion": "Pórticos Excéntricamente Arriostrados", "R0": 8},

&nbsp;   {"Sistema": "Porticos\_CA", "Descripcion": "Pórticos de Concreto Armado", "R0": 8},

&nbsp;   {"Sistema": "Dual", "Descripcion": "Sistema dual", "R0": 7},

&nbsp;   {"Sistema": "Muros", "Descripcion": "Muros estructurales", "R0": 6},

&nbsp;   {"Sistema": "Muros\_Duct\_Lim", "Descripcion": "Muros de ductilidad limitada", "R0": 4},

&nbsp;   {"Sistema": "Albañileria", "Descripcion": "Albañilería Armada o Confinada", "R0": 3},

&nbsp;   {"Sistema": "Madera", "Descripcion": "Madera", "R0": 7, "Nota": "(\*\*) diseño por esfuerzos admisibles"}

&nbsp; ]

}

TABLA N° 8 – IRREGULARIDADES ESTRUCTURALES EN ALTURA (Ia)

{

&nbsp; "Tabla\_008\_Irregularidades\_Altura": \[

&nbsp;   {

&nbsp;     "Nombre": "Irregularidad de Rigidez – Piso Blando",

&nbsp;     "Factor\_Ia": 0.75,

&nbsp;     "Descripcion": "Existe irregularidad de rigidez cuando, en cualquiera de las direcciones de análisis, en un entrepiso la rigidez lateral es menor que 70% de la rigidez lateral del entrepiso inmediato superior, o es menor que 80% de la rigidez lateral promedio de los tres niveles superiores adyacentes."

&nbsp;   },

&nbsp;   {

&nbsp;     "Nombre": "Irregularidades de Resistencia – Piso Débil",

&nbsp;     "Factor\_Ia": 0.75,

&nbsp;     "Descripcion": "Existe irregularidad de resistencia cuando, en cualquiera de las direcciones de análisis, la resistencia de un entrepiso frente a fuerzas cortantes es inferior a 80% de la resistencia del entrepiso inmediato superior."

&nbsp;   },

&nbsp;   {

&nbsp;     "Nombre": "Irregularidad Extrema de Rigidez",

&nbsp;     "Factor\_Ia": 0.50,

&nbsp;     "Descripcion": "Existe irregularidad extrema de rigidez cuando, en cualquiera de las direcciones de análisis, en un entrepiso la rigidez lateral es menor que 60% de la rigidez lateral del entrepiso inmediato superior, o es menor que 70% de la rigidez lateral promedio de los tres niveles superiores adyacentes."

&nbsp;   },

&nbsp;   {

&nbsp;     "Nombre": "Irregularidad Extrema de Resistencia",

&nbsp;     "Factor\_Ia": 0.50,

&nbsp;     "Descripcion": "Existe irregularidad extrema de resistencia cuando, en cualquiera de las direcciones de análisis, la resistencia de un entrepiso frente a fuerzas cortantes es inferior a 65% de la resistencia del entrepiso inmediato superior."

&nbsp;   },

&nbsp;   {

&nbsp;     "Nombre": "Irregularidad de Masa o Peso",

&nbsp;     "Factor\_Ia": 0.90,

&nbsp;     "Descripcion": "Se tiene irregularidad de masa (o peso) cuando el peso de un piso es mayor que 1,5 veces el peso del piso adyacente. No aplica en azoteas ni en sótanos."

&nbsp;   },

&nbsp;   {

&nbsp;     "Nombre": "Irregularidad Geométrica Vertical",

&nbsp;     "Factor\_Ia": 0.90,

&nbsp;     "Descripcion": "La configuración es irregular cuando, en cualquiera de las direcciones de análisis, la dimensión en planta de la estructura resistente a cargas laterales es mayor que 1,3 veces la correspondiente dimensión en un piso adyacente. No aplica en azoteas ni en sótanos."

&nbsp;   },

&nbsp;   {

&nbsp;     "Nombre": "Discontinuidad en los Sistemas Resistentes",

&nbsp;     "Factor\_Ia": 0.80,

&nbsp;     "Descripcion": "La estructura es irregular cuando un elemento resiste más de 10% de la fuerza cortante y se presenta un desalineamiento vertical mayor al 25% de la dimensión del elemento."

&nbsp;   },

&nbsp;   {

&nbsp;     "Nombre": "Discontinuidad Extrema de los Sistemas Resistentes",

&nbsp;     "Factor\_Ia": 0.60,

&nbsp;     "Descripcion": "Existe discontinuidad extrema cuando la fuerza cortante que resistirían los elementos discontinuos supera el 25% de la fuerza cortante total."

&nbsp;   }

&nbsp; ]

}

TABLA N° 9 – IRREGULARIDADES ESTRUCTURALES EN PLANTA (Ip)



{

&nbsp; "Tabla\_009\_Irregularidades\_Planta": \[

&nbsp;   {

&nbsp;     "Nombre": "Irregularidad Torsional",

&nbsp;     "Factor\_Ip": 0.75,

&nbsp;     "Descripcion": "Existe cuando Δmax > 1.3 Δprom en un entrepiso, considerando excentricidad accidental. Aplica solo con diafragmas rígidos y cuando Δmax > 50% del desplazamiento permisible."

&nbsp;   },

&nbsp;   {

&nbsp;     "Nombre": "Irregularidad Torsional Extrema",

&nbsp;     "Factor\_Ip": 0.60,

&nbsp;     "Descripcion": "Existe cuando Δmax > 1.5 Δprom en un entrepiso, considerando excentricidad accidental. Misma condición de diafragmas rígidos."

&nbsp;   },

&nbsp;   {

&nbsp;     "Nombre": "Esquinas Entrantes",

&nbsp;     "Factor\_Ip": 0.90,

&nbsp;     "Descripcion": "La estructura se califica como irregular cuando tiene esquinas entrantes cuyas dimensiones son mayores que 20% de la dimensión total en planta."

&nbsp;   },

&nbsp;   {

&nbsp;     "Nombre": "Discontinuidad del Diafragma",

&nbsp;     "Factor\_Ip": 0.85,

&nbsp;     "Descripcion": "Se califica como irregular cuando los diafragmas tienen discontinuidades abruptas o variaciones importantes en rigidez, incluyendo aberturas mayores que 50% del área bruta del diafragma o secciones con área neta resistente < 25%."

&nbsp;   },

&nbsp;   {

&nbsp;     "Nombre": "Sistemas no Paralelos",

&nbsp;     "Factor\_Ip": 0.90,

&nbsp;     "Descripcion": "Existe irregularidad cuando elementos resistentes a fuerzas laterales no son paralelos. No aplica si los ángulos < 30° o si elementos no paralelos resisten < 10% de la fuerza cortante."

&nbsp;   }

&nbsp; ]

}







/////////////////////////////////

\## Tabla N° 11  

\### LÍMITES PARA LA DISTORSIÓN DEL ENTREPISO  

\*(Δi / hei)\*



| Material Predominante                                   | Límite (Δi / hei) |

|----------------------------------------------------------|-------------------|

| Concreto Armado                                          | 0.007             |

| Acero                                                    | 0.010             |

| Albañilería                                              | 0.005             |

| Madera                                                   | 0.010             |

| Edificios de concreto armado con muros de ductilidad limitada | 0.005       |



\*\*Nota:\*\* Los límites de distorsión (deriva) para estructuras de uso industrial son establecidos por el proyectista, pero en ningún caso exceden el doble de los valores de esta Tabla.



